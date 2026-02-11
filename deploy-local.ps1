# Local deployment script for ModVerify to test the update feature.
# This script builds the application, creates an update manifest, and "deploys" it to a local directory.

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
if ([string]::IsNullOrEmpty($root)) { $root = Get-Location }

$deployRoot = Join-Path $root ".local_deploy"
$stagingDir = Join-Path $deployRoot "staging"
$serverDir = Join-Path $deployRoot "server"
$installDir = Join-Path $deployRoot "install"

$toolProj = Join-Path $root "src\ModVerify.CliApp\ModVerify.CliApp.csproj"
$creatorProj = Join-Path $root "modules\ModdingToolBase\src\AnakinApps\ApplicationManifestCreator\ApplicationManifestCreator.csproj"
$uploaderProj = Join-Path $root "modules\ModdingToolBase\src\AnakinApps\FtpUploader\FtpUploader.csproj"

$toolExe = "ModVerify.exe"
$updaterExe = "AnakinRaW.ExternalUpdater.exe"
$manifestCreatorDll = "AnakinRaW.ApplicationManifestCreator.dll"
$uploaderDll = "AnakinRaW.FtpUploader.dll"

# 1. Clean and Create directories
if (Test-Path $deployRoot) { Remove-Item -Recurse -Force $deployRoot }
New-Item -ItemType Directory -Path $stagingDir | Out-Null
New-Item -ItemType Directory -Path $serverDir | Out-Null
New-Item -ItemType Directory -Path $installDir | Out-Null

Write-Host "--- Building ModVerify (net481) ---" -ForegroundColor Cyan
dotnet build $toolProj --configuration Release -f net481 --output "$deployRoot\bin\tool" /p:DebugType=None /p:DebugSymbols=false

Write-Host "--- Building Manifest Creator ---" -ForegroundColor Cyan
dotnet build $creatorProj --configuration Release --output "$deployRoot\bin\creator"

Write-Host "--- Building Local Uploader ---" -ForegroundColor Cyan
dotnet build $uploaderProj --configuration Release --output "$deployRoot\bin\uploader"

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

# 4. "Deploy" to server using the local uploader
Write-Host "--- Deploying to Local Server ---" -ForegroundColor Cyan
dotnet "$deployRoot\bin\uploader\$uploaderDll" local --base "$serverDir" --source "$stagingDir"

# 5. Setup a "test" installation
Write-Host "--- Setting up Test Installation ---" -ForegroundColor Cyan
Copy-Item "$deployRoot\bin\tool\*" $installDir -Recurse

Write-Host "`nLocal deployment complete!" -ForegroundColor Green
Write-Host "Server directory: $serverDir"
Write-Host "Install directory: $installDir"
Write-Host "`nTo test the update:"
Write-Host "1. (Optional) Modify the version in version.json and run this script again to 'push' a new version to the local server."
Write-Host "2. Run ModVerify from the install directory with the following command:"
Write-Host "   cd '$installDir'"
Write-Host "   .\ModVerify.exe updateApplication --updateManifestUrl '$serverUri'"
Write-Host "`n   Note: You can also specify a different branch using --updateBranch if needed."
