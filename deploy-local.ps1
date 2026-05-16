# Local deployment script for ModVerify to test the update feature.
# This script builds the application twice at different versions, creates an update manifest
# for the newer one, and stages an "installed" copy of the older one — so triggering the
# update flow against the local server actually finds an update.

param(
    # Version baked into the "already installed" copy. Must be lower than $ServerVersion
    # so the updater treats the server build as newer.
    [string]$InstalledVersion = "0.0.1-local",

    # Version baked into the build that ends up on the local "server" / in the manifest.
    [string]$ServerVersion = "0.0.2-local"
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
if ([string]::IsNullOrEmpty($root)) { $root = Get-Location }

$deployRoot = Join-Path $root ".local_deploy"
$stagingDir = Join-Path $deployRoot "staging"
$serverDir = Join-Path $deployRoot "server"
$installDir = Join-Path $deployRoot "install"

$toolProj = Join-Path $root "src\ModVerify.CliApp\ModVerify.CliApp.csproj"
$creatorProj = Join-Path $root "modules\ModdingToolBase\src\AnakinApps\ApplicationManifestCreator\ApplicationManifestCreator.csproj"
$signerProj = Join-Path $root "modules\ModdingToolBase\src\AnakinApps\ApplicationManifestSigner\ApplicationManifestSigner.csproj"
$uploaderProj = Join-Path $root "modules\ModdingToolBase\src\AnakinApps\FtpUploader\FtpUploader.csproj"

$toolExe = "ModVerify.exe"
$updaterExe = "AnakinRaW.ExternalUpdater.exe"
$manifestCreatorDll = "AnakinRaW.ApplicationManifestCreator.dll"
$manifestSignerDll = "AnakinRaW.ApplicationManifestSigner.dll"
$uploaderDll = "AnakinRaW.FtpUploader.dll"

$devPfx = Join-Path $deployRoot "dev-signing.pfx"
$devCer = Join-Path $deployRoot "dev-trust.cer"
$devPwd = "devpass"

$versionJsonPath = Join-Path $root "version.json"
$versionJsonBackup = [IO.File]::ReadAllText($versionJsonPath)

function Set-NbgvVersion {
    param([string]$Version)
    $json = $versionJsonBackup | ConvertFrom-Json
    $json.version = $Version
    # publicReleaseRefSpec defaults the build to non-public; clearing it gives us a clean
    # "X.Y.Z" InformationalVersion locally without the +gitHash height suffix making
    # comparisons noisier than they need to be.
    if ($json.PSObject.Properties.Name -contains 'publicReleaseRefSpec') {
        $json.publicReleaseRefSpec = @()
    }
    ($json | ConvertTo-Json -Depth 32) | Set-Content -Path $versionJsonPath -Encoding UTF8
}

try {

# 1. Clean and Create directories
if (Test-Path $deployRoot) { Remove-Item -Recurse -Force $deployRoot }
New-Item -ItemType Directory -Path $stagingDir | Out-Null
New-Item -ItemType Directory -Path $serverDir | Out-Null
New-Item -ItemType Directory -Path $installDir | Out-Null

Write-Host "--- Building ModVerify (net481) @ installed v$InstalledVersion ---" -ForegroundColor Cyan
Set-NbgvVersion -Version $InstalledVersion
dotnet build $toolProj --configuration Release -f net481 --output "$deployRoot\bin\install" /p:DebugType=None /p:DebugSymbols=false /p:LocalDeploy=true

Write-Host "--- Building ModVerify (net481) @ server v$ServerVersion ---" -ForegroundColor Cyan
Set-NbgvVersion -Version $ServerVersion
dotnet build $toolProj --configuration Release -f net481 --output "$deployRoot\bin\tool" /p:DebugType=None /p:DebugSymbols=false /p:LocalDeploy=true

Write-Host "--- Building Manifest Creator ---" -ForegroundColor Cyan
dotnet build $creatorProj --configuration Release --output "$deployRoot\bin\creator"

Write-Host "--- Building Manifest Signer ---" -ForegroundColor Cyan
dotnet build $signerProj --configuration Release --output "$deployRoot\bin\signer"

Write-Host "--- Building Local Uploader ---" -ForegroundColor Cyan
dotnet build $uploaderProj --configuration Release --output "$deployRoot\bin\uploader"

Write-Host "--- Generating dev signing cert ---" -ForegroundColor Cyan
$curve = [System.Security.Cryptography.ECCurve]::CreateFromFriendlyName("nistP256")
$ecdsa = [System.Security.Cryptography.ECDsa]::Create($curve)
$req = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new(
    "CN=ModVerify Dev Signing",
    $ecdsa,
    [System.Security.Cryptography.HashAlgorithmName]::SHA256)
$cert = $req.CreateSelfSigned(
    [DateTimeOffset]::UtcNow.AddDays(-1),
    [DateTimeOffset]::UtcNow.AddYears(10))
[IO.File]::WriteAllBytes($devPfx, $cert.Export(
    [System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, $devPwd))
[IO.File]::WriteAllBytes($devCer, $cert.Export(
    [System.Security.Cryptography.X509Certificates.X509ContentType]::Cert))
$cert.Dispose()
$ecdsa.Dispose()

# 2. Prepare staging
Write-Host "--- Preparing Staging ---" -ForegroundColor Cyan
Copy-Item "$deployRoot\bin\tool\$toolExe" $stagingDir
Copy-Item "$deployRoot\bin\tool\$updaterExe" $stagingDir

# 3. Create Manifest
# Origin must be an absolute URI for the manifest creator.
# Using 127.0.0.1 and file:// is tricky with Flurl/DownloadManager sometimes. 
# We'll use the local path and ensure it's formatted correctly.
$serverPath = (Resolve-Path $serverDir).Path
$serverUri = "file:///$($serverPath.Replace('\', '/'))"
# If we have 3 slashes, Flurl/DownloadManager might still fail on Windows if it expects a certain format.
# However, the ManifestCreator just needs a valid URI for the 'Origin' field in the manifest.
Write-Host "--- Creating Manifest (Origin: $serverUri) ---" -ForegroundColor Cyan
dotnet "$deployRoot\bin\creator\$manifestCreatorDll" `
    -a "$stagingDir\$toolExe" `
    --appDataFiles "$stagingDir\$updaterExe" `
    --origin "$serverUri" `
    -o "$stagingDir" `
    -b "beta"

Write-Host "--- Signing Manifest ---" -ForegroundColor Cyan
dotnet "$deployRoot\bin\signer\$manifestSignerDll" `
    --manifest "$stagingDir\manifest.json" `
    --pfx $devPfx `
    --password $devPwd

# 4. "Deploy" to server using the local uploader
Write-Host "--- Deploying to Local Server ---" -ForegroundColor Cyan
dotnet "$deployRoot\bin\uploader\$uploaderDll" local --base "$serverDir" --source "$stagingDir"

# 5. Setup a "test" installation — uses the older-version build so the updater sees the
#    staged server build as an upgrade.
Write-Host "--- Setting up Test Installation (v$InstalledVersion) ---" -ForegroundColor Cyan
Copy-Item "$deployRoot\bin\install\*" $installDir -Recurse

Write-Host "`nLocal deployment complete!" -ForegroundColor Green
Write-Host "Installed version: $InstalledVersion"
Write-Host "Server version:    $ServerVersion"
Write-Host "Server directory:  $serverDir"
Write-Host "Install directory: $installDir"
Write-Host "`nTo test the update:"
Write-Host "1. Run ModVerify from the install directory with the following command:"
Write-Host "   cd '$installDir'"
Write-Host "   .\ModVerify.exe updateApplication --updateBranch beta --updateServerUrl '$serverUri'"
Write-Host "`n   Note: --updateServerUrl takes a server base URL and resolves to <server>/<branch>/manifest.json."
Write-Host "         Use --updateManifestUrl instead if you want to point directly at a full manifest URL."
Write-Host "`n2. To re-test, just rerun this script — every run produces v$InstalledVersion installed against v$ServerVersion on the server."
Write-Host "   Override with -InstalledVersion / -ServerVersion to exercise other version transitions."

}
finally {
    # Always restore version.json verbatim (bytes-in == bytes-out), even if a build step above failed.
    [IO.File]::WriteAllText($versionJsonPath, $versionJsonBackup)
}
