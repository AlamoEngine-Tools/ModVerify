# Cert / Key Operations Playbook

Step-by-step procedures for every cert operation in the ModVerify update-signing chain.
Background and rationale: `update-signing.md`.

**Trust hierarchy at a glance:**

```
Root CA          — 20-year self-signed, private key kept offline by the maintainer
   │ signs
   ▼
Intermediate     — 1-year, private key in GitHub Secrets, used by CI
   │ signs
   ▼
Release manifest — one per release
```

Clients embed only the **root** public cert. The verifier chains every manifest's signing
cert back to that root via `X509Chain` with `CustomRootTrust` (the Windows cert store is
never consulted).

---

## Non-negotiable rules

- **Never touch the Windows certificate store.** No `Cert:\…`, no
  `New-SelfSignedCertificate`, no `-CertStoreLocation` / `-TrustRoot` flags.
- **Never persist trust state to writable disk** on the user's machine.
- **Generate all certs in memory** via `CertificateRequest.CreateSelfSigned` /
  `CertificateRequest.Create(issuerCert, …)`. Export straight to `.pfx` / `.cer` files.
- **The root private key never leaves the offline machine.** No CI, no cloud sync, no
  screenshot.
- **Intermediate private key is allowed in GitHub Secrets.** It has a short lifetime by
  design.
- **All scripts assume PowerShell 7.5+ / .NET 9+** (`X509CertificateLoader.LoadPkcs12`).

---

## 1. Initial root cert generation (one-time)

**When:** Before shipping the first signed release. Once, ever (except for catastrophic
root rotation — section 5).

**Where:** Air-gapped or at least offline machine.

**Produces:**
- `modverify-root.pfx` — root private key + cert, password-protected. **Never networked.**
- `modverify-trust.cer` — root public cert. Commits to `src/ModVerify.CliApp/Resources/Certs/`.

### Script

```powershell
$pwd = Read-Host "Root PFX password (write this down — losing it = losing the root)" -AsSecureString

$ecdsa = [System.Security.Cryptography.ECDsa]::Create(
    [System.Security.Cryptography.ECCurve+NamedCurves]::nistP256)
try {
    $req = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new(
        "CN=ModVerify Root CA",
        $ecdsa,
        [System.Security.Cryptography.HashAlgorithmName]::SHA256)

    # Basic Constraints: CA=true, end-entity intermediates not sub-CAs
    $req.CertificateExtensions.Add(
        [System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension]::new(
            $true, $true, 0, $true))

    # Key Usage: signs other certs
    $req.CertificateExtensions.Add(
        [System.Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new(
            [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyCertSign,
            $true))

    # Subject Key Identifier — helps X509Chain link intermediates to this root
    $req.CertificateExtensions.Add(
        [System.Security.Cryptography.X509Certificates.X509SubjectKeyIdentifierExtension]::new(
            $req.PublicKey, $false))

    $notBefore = [DateTimeOffset]::UtcNow.AddMinutes(-5)
    $notAfter  = [DateTimeOffset]::UtcNow.AddYears(20)
    $cert = $req.CreateSelfSigned($notBefore, $notAfter)
    try {
        [IO.File]::WriteAllBytes(".\modverify-root.pfx", $cert.Export(
            [System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, $pwd))
        [IO.File]::WriteAllBytes(".\modverify-trust.cer", $cert.Export(
            [System.Security.Cryptography.X509Certificates.X509ContentType]::Cert))
        Write-Host "Root generated. Thumbprint: $($cert.Thumbprint)"
    } finally {
        $cert.Dispose()
    }
} finally {
    $ecdsa.Dispose()
}
```

### After

1. Strong passphrase (Diceware, ≥60 bits entropy). Memorize *and* record it.
2. **Back up the `.pfx` in at least two independent failure modes**:
   - Password manager (1Password / Bitwarden / KeePass), encrypted entry.
   - YubiKey PIV slot, or offline USB stored physically secured (safe, deposit box).
   - Optional third: printed base64 + passphrase, sealed envelope, separate location.
3. Commit `modverify-trust.cer` to `src/ModVerify.CliApp/Resources/Certs/`.
4. **Delete the working-copy `.pfx`** once backups are confirmed.
5. Schedule the annual test ceremony (section 4) on a fixed date.

---

## 2. Issuing an intermediate (recurring)

**When:**
- ~2 months before current intermediate expires (so there's overlap).
- Immediately on suspected CI compromise.

**Where:** Offline (or at minimum, disconnected) machine.

**Produces:**
- `modverify-int-YYYYMM.pfx` — intermediate keypair, password-protected, goes to GitHub
  Secrets.

### Script

```powershell
$rootPfxPath = ".\modverify-root.pfx"
$rootPwd = Read-Host "Root PFX password" -AsSecureString
$intPwd  = Read-Host "Intermediate PFX password (will go to GitHub Secrets)" -AsSecureString

$rootPwdPlain = [System.Net.NetworkCredential]::new("", $rootPwd).Password

$rootBytes = [IO.File]::ReadAllBytes($rootPfxPath)
$rootCert = [System.Security.Cryptography.X509Certificates.X509CertificateLoader]::LoadPkcs12(
    $rootBytes,
    $rootPwdPlain,
    [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::EphemeralKeySet)
$rootPwdPlain = $null

$intEcdsa = [System.Security.Cryptography.ECDsa]::Create(
    [System.Security.Cryptography.ECCurve+NamedCurves]::nistP256)
try {
    $intReq = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new(
        "CN=ModVerify Signing $((Get-Date -Format yyyy-MM))",
        $intEcdsa,
        [System.Security.Cryptography.HashAlgorithmName]::SHA256)

    # Basic Constraints: end-entity, not a sub-CA
    $intReq.CertificateExtensions.Add(
        [System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension]::new(
            $false, $false, 0, $true))

    # Key Usage: signs data (manifests), not certs
    $intReq.CertificateExtensions.Add(
        [System.Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new(
            [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature,
            $true))

    # Subject Key Identifier
    $intReq.CertificateExtensions.Add(
        [System.Security.Cryptography.X509Certificates.X509SubjectKeyIdentifierExtension]::new(
            $intReq.PublicKey, $false))

    # Sign with root
    $notBefore = [DateTimeOffset]::UtcNow.AddMinutes(-5)
    $notAfter  = [DateTimeOffset]::UtcNow.AddYears(1)
    $serial    = [System.BitConverter]::GetBytes([DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds())
    $intCert = $intReq.Create($rootCert, $notBefore, $notAfter, $serial)

    $intWithKey = $intCert.CopyWithPrivateKey($intEcdsa)
    try {
        $outPath = ".\modverify-int-$((Get-Date -Format yyyyMM)).pfx"
        [IO.File]::WriteAllBytes($outPath, $intWithKey.Export(
            [System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, $intPwd))
        Write-Host "Intermediate written to $outPath"
        Write-Host "Subject:     $($intCert.Subject)"
        Write-Host "Thumbprint:  $($intCert.Thumbprint)"
        Write-Host "Valid until: $($intCert.NotAfter.ToString('u'))"
    } finally {
        $intWithKey.Dispose()
        $intCert.Dispose()
    }
} finally {
    $intEcdsa.Dispose()
    $rootCert.Dispose()
}
```

### After

1. Base64 the intermediate PFX:
   ```powershell
   [Convert]::ToBase64String([IO.File]::ReadAllBytes(".\modverify-int-YYYYMM.pfx")) `
       | Set-Clipboard
   ```
2. In GitHub: Settings → Secrets and variables → Actions:
   - Update `UPDATER_SIGNING_PFX_B64` with the clipboard contents.
   - Update `UPDATER_SIGNING_PFX_PASSWORD` with the passphrase.
3. Wipe the local intermediate PFX:
   ```powershell
   Remove-Item ".\modverify-int-YYYYMM.pfx" -Force
   ```
4. Lock the root PFX away again.
5. Trigger a release (or wait for the next one). Confirm CI signs successfully and a
   freshly-installed client verifies the new manifest.

---

## 3. Local dev cert generation (for `deploy-local.ps1`)

Dev certs are generated fresh per `deploy-local.ps1` run — no persistence, no
backups needed.

The pattern is already implemented in `deploy-local.ps1` and follows the same
in-memory `CertificateRequest.CreateSelfSigned` shape. If `deploy-local.ps1` is rewritten
for any reason, ensure it generates a *root* + *intermediate* pair (mirroring prod), so
the local-deploy flow exercises the same chain-validation code path the prod verifier uses.

---

## 4. Annual root test ceremony

**When:** Once per year on a fixed date (e.g. every January 15). Set a calendar
reminder.

**Why:** Confirm the root key + passphrase are still accessible *before* a real
incident makes you discover otherwise. A lost root takes 6 months — 5 years to discover
during normal operation.

### Script

```powershell
$rootPwd = Read-Host "Root PFX password (annual test)" -AsSecureString
$rootPwdPlain = [System.Net.NetworkCredential]::new("", $rootPwd).Password

try {
    $rootBytes = [IO.File]::ReadAllBytes(".\modverify-root.pfx")
    $rootCert = [System.Security.Cryptography.X509Certificates.X509CertificateLoader]::LoadPkcs12(
        $rootBytes,
        $rootPwdPlain,
        [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::EphemeralKeySet)
    $rootPwdPlain = $null

    Write-Host "Root loaded:"
    Write-Host "  Subject:     $($rootCert.Subject)"
    Write-Host "  Thumbprint:  $($rootCert.Thumbprint)"
    Write-Host "  Valid until: $($rootCert.NotAfter.ToString('u'))"

    # Sign a throwaway test cert — confirms the private key actually works
    $testEcdsa = [System.Security.Cryptography.ECDsa]::Create(
        [System.Security.Cryptography.ECCurve+NamedCurves]::nistP256)
    try {
        $testReq = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new(
            "CN=Annual Test - DELETE ME",
            $testEcdsa,
            [System.Security.Cryptography.HashAlgorithmName]::SHA256)
        $serial = [System.BitConverter]::GetBytes([DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds())
        $testCert = $testReq.Create($rootCert,
            [DateTimeOffset]::UtcNow,
            [DateTimeOffset]::UtcNow.AddMinutes(5),
            $serial)
        Write-Host "Test cert signed successfully — root private key is intact."
        $testCert.Dispose()
    } finally {
        $testEcdsa.Dispose()
    }

    # Compare loaded cert against committed trust cert
    $embeddedCer = ".\src\ModVerify.CliApp\Resources\Certs\modverify-trust.cer"
    if (Test-Path $embeddedCer) {
        $embedded = [System.Security.Cryptography.X509Certificates.X509CertificateLoader]::LoadCertificate(
            [IO.File]::ReadAllBytes($embeddedCer))
        if ($embedded.Thumbprint -eq $rootCert.Thumbprint) {
            Write-Host "Embedded trust cert MATCHES the loaded root. OK."
        } else {
            Write-Warning "Embedded trust cert thumbprint DOES NOT MATCH the loaded root."
        }
        $embedded.Dispose()
    }

    $rootCert.Dispose()
    Write-Host "Annual test PASSED."
} catch {
    Write-Error "ANNUAL TEST FAILED. Recovery may be required (see section 5)."
    throw
}
```

If this fails (wrong passphrase, corrupted PFX, missing backups all gone) → go to
section 5 immediately. **Do not** wait for the current intermediate to expire.

---

## 5. Catastrophic root rotation (recovery only)

**When:**
- Root key lost (annual test failed across all backups).
- Root key compromised (someone else has it).
- Root cert within final year of its 20-year validity (unlikely concern for a long time).

**Effect:** Auto-update is **dead** for every deployed client until they manually
reinstall. There is no graceful path. This is the failure mode the design accepts in
exchange for not needing constant trust-store ceremonies.

### Procedure

1. **Generate a new root** offline (section 1, using a different filename
   `modverify-root-v2.pfx`).
2. **Issue a first intermediate under the new root** (section 2, signed by the new root).
3. **Update `src/ModVerify.CliApp/Resources/Certs/modverify-trust.cer`** to the new root's
   public cert. (Drop the old root entirely — the new root is the only trust anchor.)
4. **Update GitHub Secrets** to the new intermediate's PFX/password.
5. **Cut a release.** Auto-update is broken for all existing clients (they don't trust
   the new root). They will sit on their last installed version.
6. **Announce widely** — release notes, README, every forum the userbase reads. Make
   clear that users must manually download from GitHub Releases to recover.
7. **If the recovery is due to compromise (Scenario B)**: the malicious clients out there
   stay malicious until reinstalled. This is the irreducible blast radius.

### After

1. Resume normal operations under the new root.
2. Treat the new root with the same custody discipline as the old one (section 1 "After").
3. The old root is dead — destroy any remaining copies (overwrite, shred the printed
   backup, etc.).

---

## 6. CI intermediate compromise response

**Signs:** Unauthorized release in GitHub Actions history; signing-key leak alert;
unexplained signing activity.

### Procedure

1. **Immediately rotate the intermediate** via section 2. Use a new passphrase.
2. **Trigger a release** with a bumped version high enough to overtake any malicious
   release the attacker might publish (the rollback-rejection mechanism in
   `update-signing.md` → *Deferred update resumption* refuses anything older).
3. **Audit recent releases** since the suspected compromise window. Any unexpected
   release should be flagged and version-bumped over.
4. **The compromised intermediate's manifests stay verifiable until its `notAfter`** —
   you cannot revoke it mid-lifetime without infrastructure we don't have. Mitigation is
   the version-bump-and-overtake. The compromised intermediate will expire naturally on
   its original schedule.

### Why we don't bother with mid-lifetime revocation

A CRL or revocation list embedded in manifests is on the table per `update-signing.md`'s
"Revocation" notes, but for ModVerify scale, the simpler model is: **short intermediate
lifetimes mean compromise is bounded automatically**. Issue intermediates with a
defensible lifetime (1 year typical) and accept that lifetime as the worst-case
compromise window.

---

## Quick reference

| Operation | Section | Frequency | Where |
|---|---|---|---|
| Generate root | 1 | Once | Offline machine |
| Issue intermediate | 2 | Every ~9-10 months | Offline machine |
| Dev cert | 3 | Per `deploy-local.ps1` run | Local |
| Annual root test | 4 | Once per year | Offline machine |
| Root rotation | 5 | Never if everything works | Offline machine |
| CI compromise response | 6 | Hopefully never | Section 2 + bump + ship |
