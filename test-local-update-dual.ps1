# =========================================================================================
# Local bootstrap for the dual-deploy self-update integration test.
#
# Stages a dual-channel local deploy (primary + /v2/) via deploy-local.ps1 -DualPublish,
# then runs the shared end-to-end test against the next-generation `/v2/` server. Validates
# that a full update cycle works after the post-migration server URL base change.
#
# Optionally accepts -CompatibilityUpdater to substitute an older external-updater binary
# into the primary channel's manifest (the production migration-release shape).
#
# Independent of test-local-update.ps1.
#
# Windows-only.
# =========================================================================================

#Requires -Version 7.0

[CmdletBinding()]
param(
    [string]$InstalledVersion    = '0.0.1-local',
    [string]$ServerVersion       = '99.99.99-local',
    [string]$Branch              = 'beta',
    [string]$CompatibilityUpdater
)

$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
if ([string]::IsNullOrEmpty($root)) { $root = Get-Location }

$deployArgs = @{
    InstalledVersion = $InstalledVersion
    ServerVersion    = $ServerVersion
    DualPublish      = $true
}
if ($CompatibilityUpdater) { $deployArgs.CompatibilityUpdater = $CompatibilityUpdater }

& (Join-Path $root 'deploy-local.ps1') @deployArgs
if ($LASTEXITCODE -ne 0) { throw "deploy-local.ps1 -DualPublish failed (exit $LASTEXITCODE)." }

$nextServerDir = Join-Path $root '.local_deploy\server\v2'
if (-not (Test-Path $nextServerDir)) {
    throw "Expected /v2/ server dir at '$nextServerDir' but it does not exist."
}
$nextServerUri = "file:///$(((Resolve-Path $nextServerDir).Path -replace '\\','/'))"

& (Join-Path $root 'modules\ModdingToolBase\scripts\Test-LocalUpdateCycle.ps1') `
    -AppExePath         (Join-Path $root '.local_deploy\install\ModVerify.exe') `
    -ServerUri          $nextServerUri `
    -Branch             $Branch `
    -NoUpdateMessage    'No update available.' `
    -ExpectedNewVersion $ServerVersion

exit $LASTEXITCODE
