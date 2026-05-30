param(
    [switch]$IncludeArtifacts
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$paths = @(
    "logs",
    "downloads",
    "backups"
)

if ($IncludeArtifacts) {
    $paths += "artifacts"
}

foreach ($path in $paths) {
    $fullPath = Join-Path $root $path
    if (Test-Path -LiteralPath $fullPath) {
        Remove-Item -LiteralPath $fullPath -Recurse -Force
        Write-Host "Removed $fullPath"
    }
}
