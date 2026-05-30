param(
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$tag = "v$Version"
$releaseDir = Join-Path $root "artifacts\releases\$tag"
$localDir = Join-Path $root "artifacts\local-release\$tag"
$packageName = "NFOX.UpdatePackage-$Version.zip"

function Get-FileUri {
    param([string]$Path)
    $resolved = (Resolve-Path -LiteralPath $Path).Path
    return (New-Object System.Uri($resolved)).AbsoluteUri
}

& (Join-Path $PSScriptRoot "build-release.ps1") -Version $Version
if ($LASTEXITCODE -ne 0) {
    throw "Release build failed."
}

Remove-Item -LiteralPath $localDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $localDir -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $releaseDir $packageName) -Destination $localDir -Force
Copy-Item -LiteralPath (Join-Path $releaseDir "checksums.txt") -Destination $localDir -Force

$manifest = Get-Content -LiteralPath (Join-Path $releaseDir "manifest.json") -Raw | ConvertFrom-Json
$manifest.packages.updatePackage.downloadUrl = Get-FileUri -Path (Join-Path $localDir $packageName)
$manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $localDir "manifest.json") -Encoding UTF8

Write-Host "Local release created in $localDir"
Write-Host "Manifest URL:"
Write-Host (Get-FileUri -Path (Join-Path $localDir "manifest.json"))
