param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$tag = "v$Version"
$releaseDir = Join-Path $root "artifacts\releases\$tag"
$localDir = Join-Path $root "artifacts\local-release\$tag"

function Get-FileUri {
    param([string]$Path)
    $resolved = (Resolve-Path -LiteralPath $Path).Path
    return (New-Object System.Uri($resolved)).AbsoluteUri
}

& (Join-Path $PSScriptRoot "build-release.ps1") -Version $Version

Remove-Item -LiteralPath $localDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $localDir -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $releaseDir "NFOX.DemoApp-$Version.zip") -Destination $localDir -Force
Copy-Item -LiteralPath (Join-Path $releaseDir "NFOX.Migrations-$Version.zip") -Destination $localDir -Force
Copy-Item -LiteralPath (Join-Path $releaseDir "checksums.txt") -Destination $localDir -Force

$manifest = Get-Content -LiteralPath (Join-Path $releaseDir "manifest.json") -Raw | ConvertFrom-Json
$manifest.packages.app.downloadUrl = Get-FileUri -Path (Join-Path $localDir "NFOX.DemoApp-$Version.zip")
$manifest.packages.migrations.downloadUrl = Get-FileUri -Path (Join-Path $localDir "NFOX.Migrations-$Version.zip")
$manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $localDir "manifest.json") -Encoding UTF8

Write-Host "Local release created in $localDir"
Write-Host "Manifest URL:"
Write-Host (Get-FileUri -Path (Join-Path $localDir "manifest.json"))
