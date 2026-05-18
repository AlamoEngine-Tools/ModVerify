# Update Signing & Updater Hardening

Single source of truth for how ModVerify's updater authenticates manifests, files, and the
external-updater binary; how trust is bootstrapped and rotated; and what's still TODO.

---

## Goal

Every update artifact a ModVerify client acts on is authenticated against a trust anchor pinned
in the host at build time. Compromise of the CDN, MITM on the download channel, accidental
corruption of staged artifacts, or local tampering of files between download and install must
abort the update, not let it proceed.

## Threat model — what we want to prevent

In order of decreasing attacker capability:

1. **Compromised CDN serving signed manifests pointing at malicious bytes.** Out of scope — the
   bytes still have to match the signed manifest's hash, and we don't sign attacker bytes.
2. **Local attacker with write access to the install directory.** Can swap
   `AnakinRaW.ExternalUpdater.exe` for arbitrary code, which the main app then launches with
   the user's privileges (or elevated). Closed by *External updater hardening* —
   manifest-hash verification of the updater bytes before launch.
3. **Local attacker with write access to `%TEMP%` and the download repository.** Can modify
   the update-info file between writer and reader (TOCTOU), or swap a downloaded-and-verified
   source blob between download and the updater's file-move. Closed by *Deferred update
   resumption* — `--updatePayload` on the command line replaces the tempfile, and the updater
   re-hashes every source before moving.
4. **Network-active attacker without our signing key.** Can replay older signed manifests
   (rollback) to pin users on a known-vulnerable version. Closed by *Deferred update
   resumption* — rollback rejection on both CDN-fetched and pending-on-disk manifests.

What's already done covers (1). (2)-(4) is the work remaining.

### Threats out of scope

- Attacker who can read process memory.
- Attacker who can write to `%ProgramFiles%` directly (DLL planting next to our exe, loader
  hijack — OS-level integrity problem, not something file signing fixes).
- Compromise of the **offline root** key. Catastrophic — requires shipping a new release
  with a new embedded root and having every user manually reinstall (`docs/cert-playbook.md`
  §5). The root is kept offline specifically to make this hard. **CI intermediate
  compromise is in-scope** and handled by routine rotation (`docs/cert-playbook.md` §6):
  issue a new intermediate, ship the next release, accept that manifests signed by the
  compromised intermediate stay verifiable until its `notAfter` (worst-case window
  bounded by intermediate lifetime, typically 1 year).

---

## What's implemented today

- **Manifest signing.** Every manifest the release pipeline publishes is signed with a self-signed
  ECDSA P-256 cert held in GitHub Actions secrets. Signature is a `signature` block embedded in
  the JSON: `{ alg, value, cert }`. One signature per manifest, made with the current active key.
- **Verification on the client.** Default policy is `SignaturePolicy.Required`. The fetch path
  is owned by the framework's internal `ManifestFetcher`; hosts can't bypass it. The verifier
  chain is `ManifestLoaderBase.LoadAndVerifyManifest` → `ISignatureVerifier.Verify` →
  `ICertificateStore`. All five contracts (`ICertificateStore`, `ISignatureVerifier`,
  `ManifestLoaderBase`, `IManifestLoaderProvider`, `IManifestFetcher`) are unfakeable from
  outside the framework assembly.
- **Mirror failover.** Signature failure on one mirror tries the next; only after all mirrors
  fail does `ManifestDownloadException` surface.
- **Config guards.** `ManifestFetcher` refuses to construct when `SignaturePolicy.Required` but
  `ComponentDownloadConfiguration.ValidationPolicy != Required` (the signed manifest would be
  moot if components weren't hash-checked). `ManifestLoaderBase` refuses a verified manifest in
  which any `InstallableComponent` lacks integrity info.
- **`CertificateManager` (in `ApplicationBase`).** Loads trust anchors from embedded resources
  and from a local-deploy dev cert path. Refuses any cert that carries a private key (no PFX
  leakage into the consumer build).
- **CI guards.** `release.yml`'s `deploy` job verifies the embedded trust cert: must be a valid
  X.509, must be public-only. Missing or PFX-shaped cert fails the deploy.
- **Local-deploy support.** `deploy-local.ps1` generates a throwaway dev cert per run (via
  in-memory `CertificateRequest`, never touches the Windows cert store), signs the local
  manifest with it, stages the public half next to the install dir. The `LOCAL_DEPLOY` MSBuild
  symbol switches on the dev-cert lookup in `Program.RegisterTrustedCertificates`.
- **Production prod cert is not yet generated.** When generated, dropping
  `modverify-trust.cer` into `src/ModVerify.CliApp/Resources/Certs/` activates the release
  pipeline end-to-end. Setup steps below.

## Manifest format

```json
{
  "name": "ModVerify",
  "version": "...",
  "branch": "stable",
  "components": [
    {
      "id": "...",
      "originInfo": { "url": "...", "size": ..., "sha256": "..." },
      ...
    }
  ],
  "signature": {
    "alg": "ES256",
    "value": "<base64 ECDSA signature>",
    "cert": "<base64 DER X.509 cert — the intermediate that signed this manifest>"
  }
}
```

The signature covers the canonical bytes produced by serializing the manifest with
`signature = null` and the framework's `ManifestJsonOptions.Default`. Both signer and verifier
use the same canonicalizer (`CanonicalManifestSerializer.SerializeForDigest`) so the digest is
byte-stable.

`signature.cert` is the **intermediate** that signed this manifest — not a self-contained
trust anchor. The verifier builds a chain from this cert to a root in the build-embedded
trust set (see *Trust bootstrap and rotation*) and rejects any manifest whose intermediate
isn't currently within its validity window.

The verifier reads the algorithm from the manifest and dispatches per `SignatureAlgorithm`
(JWS-style: `ES256`, `ES384`, `ES512`). Future algorithms slot in non-breakingly.

## Three-layer integrity

- **Chain layer.** The cert in `signature.cert` (an intermediate) must chain to a root in
  the build-embedded trust set and be temporally valid. Verified via `X509Chain` with
  `CustomTrustStore = { root }` and `TrustMode = X509ChainTrustMode.CustomRootTrust` — the
  OS cert store is bypassed entirely.
- **Manifest layer.** Signature verified with the intermediate's public key before anything
  in the manifest is trusted.
- **Component layer.** The verified manifest declares SHA-256 per component. The download
  manager validates each component's hash against the manifest's declaration at download
  time (`HashDownloadValidator`). The three layers compose: chain authenticates the
  intermediate, intermediate authenticates the manifest, manifest authenticates the
  components.

## Trust bootstrap and rotation

### Build-time pinning

The host app embeds the public **root cert** at
`src/ModVerify.CliApp/Resources/Certs/modverify-trust.cer`. The root is long-lived (20-year
validity); its private key is held offline by the maintainer and never signs a manifest
directly — it only signs intermediates. CI signs manifests with a short-lived intermediate
(typical 1-year lifetime) whose public cert travels inside each manifest's `signature.cert`
field.

The csproj has a conditional `EmbeddedResource` entry, so the build works whether or not
the file is present (today: not present, until the prod root cert is generated — see
`docs/cert-playbook.md` section 1). At app startup, `CertificateManager` reads the resource
and adds the root to the in-memory `ICertificateStore`.

The verifier uses that in-memory store as the `CustomTrustStore` for `X509Chain`, with
`TrustMode = X509ChainTrustMode.CustomRootTrust`, so the OS cert store is never consulted —
manifest verification is determined entirely by what's embedded.

The dev path (`LOCAL_DEPLOY` builds only) additionally reads `../dev-trust.cer` relative to
the running exe — that's the dev root cert `deploy-local.ps1` generates fresh on each run.

**Chain validation is NOT YET implemented.** Today's `SignatureVerifier` does a direct
fingerprint check against `ICertificateStore`, which assumes the cert in `signature.cert`
is itself trusted. Extending it to `X509Chain` validation against a root-only trust set
is ~30-50 LoC of verifier change plus tests — listed as required work for the migration
release.

---

## External updater hardening (NOT YET IMPLEMENTED)

**Scope: one principle.** Before launching `AnakinRaW.ExternalUpdater.exe`, the main app
must verify the on-disk updater bytes match the SHA-256 the signature-verified installed
manifest declares for it. That is the whole section. Everything else about the launch —
what arguments are passed, what manifest authorizes the update, what staged blobs feed into
it, how the deferred path resumes — is owned by *Deferred update resumption*.

This single check is sufficient because we already have a signed-manifest trust chain
anchored at the embedded root. The manifest declares the updater's hash as a component;
bytes that match that declaration are, by definition, the bytes the root's chain signed off
on. An Authenticode signature on the updater with `WinVerifyTrust` at launch would close
the same threat at the cost of a second trust mechanism and a parallel signing
infrastructure — strictly redundant under the current design.

The main app itself doesn't need a cert-based check either. Its integrity comes from the
same hash-chain — the updater installs main-app bytes whose SHA-256 matches the signed
manifest's declaration and then re-launches the freshly-installed binary from a file
handle it already holds. The updater is constrained by design to "apply file moves from
`--updatePayload`" and "restart the supplied `--appToStart`" — no arbitrary-target launch
capability — so subverting it into running other code is out of scope for this section:
its threat surface is bounded by those two operations, both hash- or signature-anchored.

### Current state

No hash check happens before the updater is launched. An attacker with write access to the
install directory can swap `AnakinRaW.ExternalUpdater.exe` between install time and the
next launch, and the main app executes it with the user's privileges.

### Fix

Before `Process.Start` on the updater:

1. Open the on-disk updater `FileShare.Read`-only.
2. SHA-256 the file from the open handle; compare to the hash the signature-verified
   installed manifest declares for the updater component. Mismatch → abort with a clear
   error.
3. `CreateProcess` from the verified handle so the OS resolves the executable from the
   same handle, closing the TOCTOU between hash and launch.

The release pipeline needs no code-signing step for the updater binary. The updater's
integrity is fully described by the manifest's declared hash, which the existing CI
manifest-signing step already covers via the chain.

### Migration sequencing (single release via Costura + extract)

Everything resolves in **one release**. The cross-generation handoff for the updater
binary happens automatically via the framework's existing Costura embedding +
startup-extraction pattern, not via the manifest's install flow.

How the pattern works (verified in ModdingToolBase):

- `ModVerify.CliApp.csproj`'s net481 build references `ExternalUpdater.App.csproj`.
  Costura.Fody packs the resulting `AnakinRaW.ExternalUpdater.exe` into `ModVerify.exe`
  as an embedded resource.
- At every startup, `CosturaApplicationProductService.CreateExternalUpdaterComponent()`
  compares the embedded updater's version against the on-disk updater's version and writes
  the embedded copy to disk **iff** it's newer (the `streamVersion > installedVersion`
  comparator in `CosturaApplicationProductService`).

Migration release install flow:

> Old deployed ModVerify reads R1 manifest → downloads R1 files → tries to replace
> `ModVerify.exe` → in use → delegates to the OLD updater (current deployed binary, OLD
> CLI). The OLD updater performs its file moves and exits. NEW ModVerify launches → its
> startup-extraction sees the embedded updater (NEW) is newer than the on-disk one (OLD)
> → writes the new updater to disk. From this point on every update runs through the NEW
> updater with the NEW CLI.

The NEW updater is **never invoked by an OLD main app**, because the OLD main app is
replaced in the very same install that places the NEW updater inside the new
`ModVerify.exe`. The NEW updater therefore does not need to accept the OLD CLI. Clean
break, with no compat shim and no second release.

The manifest's `updater` component is advisory under this scheme — what's load-bearing is
what's embedded in `ModVerify.exe`. Whether the OLD updater succeeds or fails at the
manifest-listed updater-component install step is immaterial; the Costura + extract path
produces the correct end-state either way.

The pattern applies to any ModdingToolBase consumer that uses Costura embedding, not just
ModVerify.

The update execution pipeline is the **same** for the immediate path and the deferred path —
the only difference is *when* it runs. Both reconstruct the install plan entirely from
authenticated on-disk state, and never trust mutable registry contents to describe what to
install. The deferred path is therefore not a separate code path — it's the same pipeline
invoked at next launch instead of immediately. "Resume" applies to both.

### Why the current registry handoff is broken

Today's `HKCU` entries (`UpdaterPath`, `UpdateCommandArgs`, `RequiresUpdate`) describe the
deferred update as "run this exe with these args" — all three fields are user-writable.
An attacker with `HKCU` write controls what the app executes next launch, no file tampering
needed. Authenticated on-disk state has to do that work instead.

### On-disk state is the source of truth

- **Pending manifest** — the signed manifest at e.g.
  `%LocalAppData%/ModVerify/pending-update/manifest.json`. Self-authenticating via its
  `signature` block, so the user-writable location is fine.
- **Staged download repository** — per-component blobs the download manager already
  hash-verified at fetch time; re-hashed when the pipeline runs.
- **Installed manifest** — signature-verified at install time; used to derive the updater
  binary path the pipeline launches (subject to Authenticode verification per *External
  updater hardening*).
- **`highest-installed-version`** — written next to the installed manifest on every
  successful install; consulted by step 2 for rollback rejection.

Registry carries only what's needed to *find* the on-disk state:

- `RequiresUpdate` — bool, the resume gate.
- `PendingManifestPath` — full path to the pending manifest on disk.
- `Branch` — branch name (e.g. `stable`), so the framework can resolve the matching staged
  download repository and re-download via the right mirror if step 3 finds gaps.

Principle: registry tells the framework what to *find*, never what to *execute*. An attacker
rewriting `PendingManifestPath` or `Branch` either points at nothing (→ fall through to a
normal launch, same as today's behavior on missing/invalid pending state) or at some other
signed manifest (→ still has to verify, and rollback rejection in step 2 catches
older-but-signed substitution). No `UpdaterPath`, no command-line args.

### Execution pipeline

Runs immediately after a fresh download completes, OR on startup with `RequiresUpdate=1`.
Steps are identical:

1. **Resolve and verify the pending manifest.** In the deferred case: read
   `PendingManifestPath` from registry. If the file or the staged download location for
   `Branch` is missing, treat exactly like today's behavior for an inconsistent pending
   state — clear the registry keys, continue with a normal launch. In the immediate case:
   the manifest path is in-process. Either way, verify via the same
   `ManifestLoaderBase.LoadAndVerifyManifest` chain used for CDN manifests; signature
   failure aborts.
2. **Reject rollback.** Compare the pending manifest's `version` to
   `highest-installed-version`. Refuse anything strictly older. Closes intentional
   downgrades, stale-mirror replay, and substitution of the pending manifest with an
   older-but-still-signed one. The "user reinstalls from GitHub" recovery path is preserved
   because the reinstall resets this state.
3. **Re-hash staged blobs** against the verified manifest. Missing or mismatched blobs are
   treated as "not staged" — gaps to fill in step 4, not errors.
4. **Compute install diff.** Per component: already installed at the right hash → skip;
   staged-and-verified → include in payload; missing/corrupted → re-download via the normal
   download manager against the manifest's `originInfo`.
5. **Build the updater payload.** Always `--updatePayload` (base64 JSON on the command
   line), never `--updateFile` — the command line travels at the same trust level as the
   launch itself; no separate tempfile to TOCTOU. Payload carries per-source-file
   `{ file, destination, sha256 }`. The updater re-hashes each source before moving it; any
   mismatch aborts the entire batch with backup restore (no partial application). The
   updater also self-checks: refuses to run if its working directory isn't a recognized
   install path.
6. **Hand off to the external updater.** Updater binary path is derived from the *installed*
   signature-verified manifest (`ExternalUpdaterService.GetExternalUpdater`) — never from
   registry. Updater bytes are hash-verified against the manifest's declared SHA-256 (via a
   file handle) and `CreateProcess` runs from that handle. See *External updater hardening*.
7. **On success**, write the new `highest-installed-version`, delete the pending manifest,
   clear registry keys, optionally prune staged blobs. On updater failure, leave state for
   next-launch retry.

Failures in 1-3 wipe pending state and fall through to a normal launch — user pays a
re-download, never an unsafe install. The only step skipped by an immediate-path invocation
relative to a deferred resume is the disk-read of the manifest (already in memory from the
fresh fetch); verification still runs.

### Cert-rotation interaction

If the trust store rotated (A → B) between defer and resume and the pending manifest was
signed by A: verification still succeeds while A remains trusted (during the transition
window described under *Cert rotation runbook*). Once A is dropped from the embedded set,
step 1 fails cleanly and resume falls through — same outcome as the "too old to auto-update"
path under *Trust bootstrap and rotation*.

---

## Migration from unsigned to signed updates (NOT YET IMPLEMENTED)

Deployed clients don't verify manifests and don't carry a trust cert. We can't retroactively
secure them. Goal: guarantee the next update any old client performs lands it on a known-good
signed build, protected from then on.

### The release being developed *is* the migration release

There is no separate "v1" path today — deployed clients fetch from a single existing path:

```
https://republicatwar.com/downloads/ModVerify/<branch>/manifest.json
```

The release we're currently developing — the one that introduces signing — publishes to
**that existing path**, exactly like every prior release. It is the *last* release
published there. The path stays frozen from then on.

What this release brings together:

1. Embedded **root trust cert** (`Resources/Certs/modverify-trust.cer`).
2. Manifest signing wired through `release.yml` (CI signs with the first intermediate).
3. Chain-validation `SignatureVerifier` change in the framework.
4. **Hash-check on updater launch** — main-app side; verifies the on-disk
   `AnakinRaW.ExternalUpdater.exe` against the signed manifest's declared SHA-256 before
   `Process.Start`, then `CreateProcess` from the verified handle.
5. **New external updater binary** with hardened CLI (`--updatePayload`, per-source-file
   hashes, working-directory self-check). Travels embedded inside `ModVerify.exe` via
   Costura; gets written to disk on first launch by the framework's existing
   extract-and-replace path. See *External updater hardening* → *Migration sequencing
   (single release via Costura + extract)*.
6. Compile-time mirror URL pointing at a **new** path, e.g.
   `downloads/ModVerify/v2/<branch>/manifest.json`.

The migration release's manifest must remain parseable by the currently-deployed (unsigned)
deserializer. `System.Text.Json` ignores unknown properties by default, so the new
`signature` block is expected to be tolerated by old clients — verify against the actually-
deployed framework version before relying on this. If the deployed parser is strict, strip
the signature from the manifest copy uploaded to the existing path.

### Cross-generation handoff via Costura + extract

The migration release ships the **new** `AnakinRaW.ExternalUpdater.exe` embedded inside
`ModVerify.exe` (Costura.Fody). It does *not* rely on the OLD updater installing itself.

The deployed (old) ModVerify is what runs the install of the migration release:

> Old ModVerify writes new files to staging → tries to replace `ModVerify.exe` → fails
> (process in use) → delegates to `AnakinRaW.ExternalUpdater.exe` using the **old** CLI.
> The OLD updater replaces `ModVerify.exe`, exits.

That's the OLD updater's whole job. It does not need to install the NEW updater binary —
the new updater arrives embedded inside the new `ModVerify.exe`. After the OLD updater
exits, the new ModVerify launches; `CosturaApplicationProductService` extracts the
embedded NEW updater on startup and writes it over the on-disk OLD updater (which is no
longer running). From the next update onward, the NEW updater is on disk and the NEW CLI
is what's used.

This collapses what would otherwise be a multi-release migration into a single release.
The build pipeline produces it the same way it produces any release — the csproj
references the new `ExternalUpdater.App` project, Costura packs it,
`ApplicationManifestCreator` writes a manifest from the staged outputs. **No JSON
hand-crafting.**

The manifest's `updater` component listing is advisory; what's load-bearing is the
embedded updater inside `ModVerify.exe`. Whether the OLD updater succeeds or fails at the
manifest-listed updater-component install step is immaterial — Costura + extract produces
the same end-state regardless.

This applies to any ModdingToolBase-based app that uses the Costura+extract pattern, not
just ModVerify.

### After the migration release ships

- **Old clients** check the existing path, see the migration release as their next update,
  install it. Bytes hash-verified against the manifest (no signature verification client-
  side — same trust model they've always had). Once installed, the new build's mirror URL
  is /v2/; every subsequent check happens there, signature-verified end to end.
- **Migration-release clients** (and later builds) check /v2/. Until the *next* release
  publishes there, the check legitimately returns "no update available." Not an error.
- The existing path is **never overwritten** after the migration release. Static blob; costs
  nothing to host indefinitely; remains the upgrade ramp for any old client that comes
  online months or years later.

### No dual upload

The migration release goes to the existing path. The first post-migration release goes to
/v2/. There is no moment when the same manifest is published to both paths.

### Release pipeline change (`release.yml`)

For the migration release itself: no upload-path change required — same target as today,
plus the new sign-manifest step. For the **first** post-migration release: change
`ORIGIN_BASE` and the SFTP `base_path` to point at the new /v2/ subpath. The existing upload
target is decommissioned at that point.

### Risks and edge cases

- **Pre-migration clients with the migration release pending as a deferred update at
  cutover.** They resume into the migration release, install it, and on their first /v2/
  check land on the signed channel. No special handling.
- **Transition window threat surface** is unchanged from today: pre-migration clients on
  the unsigned path remain unsigned until they update. Population shrinks as users migrate.
- **Future protocol breaks** reuse the same pattern: a future migration release introduces
  /v3/, freezes /v2/ at that point, etc. Each frozen path is a static JSON file.

---

## Operations

### Cert / key recipes → `docs/cert-playbook.md`

All cert generation, intermediate issuance, annual test, and root rotation procedures live
in `docs/cert-playbook.md`. The playbook is the step-by-step source; this doc owns the
design rationale. Cross-references:

| Operation | Playbook section |
|---|---|
| Generate the root cert (one-time, when ready to ship the first signed release) | §1 |
| Issue a new intermediate (every ~9-10 months, or on CI compromise) | §2 |
| Local-deploy dev certs | §3 |
| Annual root test ceremony | §4 |
| Catastrophic root rotation | §5 |
| CI intermediate compromise response | §6 |

GitHub Secrets the CI pipeline reads:

| Secret | Value |
|---|---|
| `UPDATER_SIGNING_PFX_B64` | Base64 of the current intermediate PFX (rotated per §2) |
| `UPDATER_SIGNING_PFX_PASSWORD` | Passphrase for the current intermediate PFX |
| `SFTP_USER` / `SFTP_PASSWORD` | (Until migration release; see migration section) |

The root PFX is **never** put into a GitHub Secret. It lives offline only.

### Cert rotation runbook

Trust hierarchy: embedded **root** (offline private key, 20-year cert) → **intermediate**
(in-CI private key, 1-year cert) → manifest signature. Rotation is split into two
fundamentally different operations:

#### Routine intermediate rotation (every ~9-10 months, or on CI compromise)

Done entirely offline. **No client coordination needed.** Procedure detailed in
`docs/cert-playbook.md` section 2; the summary:

1. Offline ceremony: load root, generate a new intermediate keypair, sign with root,
   export to PFX.
2. Push the new intermediate PFX (base64) and passphrase to GitHub Secrets.
3. Lock the root away. CI signs the next release with the new intermediate.

Every client — including those dormant for years — verifies the next release cleanly,
because the embedded root is unchanged and the new intermediate chains to it. The
previous intermediate is now retired; manifests it signed remain verifiable until its
`notAfter` passes (auto-expiry — see *Three-layer integrity*).

Pick the intermediate lifetime comfortably greater than your worst-case release gap. The
latest manifest on the server must still be signed by an unexpired intermediate when the
next dormant user shows up to check. 1 year is the default; lengthen if release cadence is
slower.

#### Root rotation (catastrophic recovery only)

Used only when the root key is lost, compromised, or within the final year of its
20-year validity. Auto-update is **dead** for every deployed client until they manually
reinstall — this is the price of not maintaining a writable on-disk trust store.

Procedure detailed in `docs/cert-playbook.md` section 5. The trade-off is intentional:
routine rotations cost nothing (offline ceremony + push to Secrets); the rare
catastrophic event costs everyone a manual reinstall.

#### Annual test ceremony

Once per year, dry-run the offline root: load it, sign a throwaway cert, confirm it
chains to the embedded trust cert, discard. See `docs/cert-playbook.md` section 4.
Confirms the root key custody is intact *before* a real incident forces the discovery.

### Local-deploy notes

`deploy-local.ps1` is independent of the prod cert. It:

- Generates `dev-signing.pfx` + `dev-trust.cer` fresh in `.local_deploy/` per run (in-memory,
  never touches the Windows cert store).
- Builds the app twice (installed and server versions) with `/p:LocalDeploy=true` so the
  `LOCAL_DEPLOY` MSBuild symbol is defined; that compiles in the dev-cert lookup path.
- Signs the local manifest with the dev pfx.
- Stages everything under `.local_deploy/` (gitignored).

The dev cert never collides with the prod cert. The `LOCAL_DEPLOY` symbol is off by default;
Release builds shipped to users carry no dev-cert lookup code.

### CI gates

- `release.yml` `deploy` job's first step verifies `src/ModVerify.CliApp/Resources/Certs/modverify-trust.cer`:
  must exist (else: fail with "generate the prod cert per docs/update-signing.md"), must parse
  as X.509, must not carry a private key. Catches the PFX-instead-of-CER mistake before any
  artifact reaches a user.
- The `pack` job and PRs are not gated, so development continues even before the prod cert is
  in place.

---

## Out of scope

- Replay/downgrade beyond the rollback-rejection step in updater hardening.
- RFC 3161 timestamps / counter-signatures.
- CRL/OCSP. Recovery from a compromised key is a planned rotation (above), not online
  revocation.
- A real CA-issued code-signing cert. Possibly future, for SmartScreen reputation. Until then,
  self-signed + manifest-anchored trust chain is the design.
- Hardening against attackers with write access to `%ProgramFiles%` itself (OS-level integrity).
