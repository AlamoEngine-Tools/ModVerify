# =========================================================================================
# Local deployment to test the ModVerify update flow end-to-end.
#
# USAGE
#   .\deploy-local.ps1                                       # single-channel test
#   .\deploy-local.ps1 -DualPublish                          # also publish to a next-gen channel
#   .\deploy-local.ps1 -DualPublish -CompatibilityUpdater <path>
#                                                            # primary uses compat updater;
#                                                            # next-gen uses build-output updater
#
#   -CompatibilityUpdater requires -DualPublish.
#   -InstalledVersion / -ServerVersion override the version pair used to set up the test.
#
# Builds ModVerify twice (older "installed" + newer "server"), then hands off to the shared
# Publish-LocalRelease.ps1 in ModdingToolBase for cert generation, manifest signing, and
# install-dir staging.
# =========================================================================================

param(
    [string]$InstalledVersion = "0.0.1-local",
    [string]$ServerVersion    = "99.99.99-local",
    [switch]$DualPublish,
    [string]$CompatibilityUpdater
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
if ([string]::IsNullOrEmpty($root)) { $root = Get-Location }

. (Join-Path $root "modules\ModdingToolBase\scripts\NbgvVersion.ps1")

$deployRoot      = Join-Path $root ".local_deploy"
$installBuildDir = Join-Path $deployRoot "bin\install"
$serverBuildDir  = Join-Path $deployRoot "bin\tool"

$toolProj   = Join-Path $root "src\ModVerify.CliApp\ModVerify.CliApp.csproj"
$baseScript = Join-Path $root "modules\ModdingToolBase\scripts\Publish-LocalRelease.ps1"

if (Test-Path $deployRoot) { Remove-Item -Recurse -Force $deployRoot }
New-Item -ItemType Directory -Path $deployRoot | Out-Null

$nbgv = Backup-NbgvVersion -RepoRoot $root
try {
    Write-Host "--- Building ModVerify (net481) @ installed v$InstalledVersion ---" -ForegroundColor Cyan
    Set-NbgvVersion -Snapshot $nbgv -Version $InstalledVersion
    dotnet build $toolProj --configuration Release -f net481 --output $installBuildDir /p:DebugType=None /p:DebugSymbols=false /p:LocalDeploy=true

    Write-Host "--- Building ModVerify (net481) @ server v$ServerVersion ---" -ForegroundColor Cyan
    Set-NbgvVersion -Snapshot $nbgv -Version $ServerVersion
    dotnet build $toolProj --configuration Release -f net481 --output $serverBuildDir /p:DebugType=None /p:DebugSymbols=false /p:LocalDeploy=true

    $publishParams = @{
        AppExePath      = Join-Path $serverBuildDir "ModVerify.exe"
        UpdaterExePath  = Join-Path $serverBuildDir "AnakinRaW.ExternalUpdater.exe"
        DeployRoot      = $deployRoot
        InstallBuildDir = $installBuildDir
        Branch          = "beta"
    }
    if ($DualPublish)          { $publishParams.DualPublish          = $true }
    if ($CompatibilityUpdater) { $publishParams.CompatibilityUpdater = $CompatibilityUpdater }

    & $baseScript @publishParams
}
finally {
    Restore-NbgvVersion -Snapshot $nbgv
}
