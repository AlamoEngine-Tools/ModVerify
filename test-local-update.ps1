# =========================================================================================
# Local bootstrap for the self-update integration test.
#
# Stages a local deploy via deploy-local.ps1, then runs the shared end-to-end test from
# ModdingToolBase against the staged install dir + signed local server.
#
# Windows-only.
# =========================================================================================

#Requires -Version 7.0

[CmdletBinding()]
param(
    [string]$InstalledVersion = '0.0.1-local',
    [string]$ServerVersion    = '99.99.99-local',
    [string]$Branch           = 'beta'
)

$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
if ([string]::IsNullOrEmpty($root)) { $root = Get-Location }

& (Join-Path $root 'deploy-local.ps1') -InstalledVersion $InstalledVersion -ServerVersion $ServerVersion
if ($LASTEXITCODE -ne 0) { throw "deploy-local.ps1 failed (exit $LASTEXITCODE)." }

$serverDir = Join-Path $root '.local_deploy\server'
$serverUri = "file:///$(((Resolve-Path $serverDir).Path -replace '\\','/'))"

& (Join-Path $root 'modules\ModdingToolBase\scripts\Test-LocalUpdateCycle.ps1') `
    -AppExePath         (Join-Path $root '.local_deploy\install\ModVerify.exe') `
    -ServerUri          $serverUri `
    -Branch             $Branch `
    -NoUpdateMessage    'No update available.' `
    -ExpectedNewVersion $ServerVersion

exit $LASTEXITCODE
