param(
    [string]$Owner = "",
    [string]$Repo = "nfox-auto-update-channel",
    [Parameter(Mandatory = $true)]
    [string]$Version,
    [string]$ReleaseTitle = "",
    [switch]$AutoCreateRepository,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

Write-Warning "publish-github-release.ps1 is retained for compatibility. Use publish-update-channel-release.ps1 for the releases-only update channel."

$arguments = @(
    "-Repo", $Repo,
    "-Version", $Version
)

if (-not [string]::IsNullOrWhiteSpace($Owner)) {
    $arguments += @("-Owner", $Owner)
}

if (-not [string]::IsNullOrWhiteSpace($ReleaseTitle)) {
    $arguments += @("-ReleaseTitle", $ReleaseTitle)
}

if ($SkipBuild) {
    $arguments += "-SkipBuild"
}

& (Join-Path $PSScriptRoot "publish-update-channel-release.ps1") @arguments
if ($LASTEXITCODE -ne 0) {
    throw "Update channel publishing failed."
}
