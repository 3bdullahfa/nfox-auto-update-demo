param(
    [string]$Owner = "",
    [string]$Repo = "nfox-auto-update-demo",
    [Parameter(Mandatory = $true)]
    [string]$Version,
    [string]$ReleaseTitle = "",
    [switch]$AutoCreateRepository,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$tag = "v$Version"

function Require-Command {
    param([string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "$Name is not installed or is not available in PATH."
    }
}

Require-Command -Name "gh"

& gh auth status | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not authenticated. Please run: gh auth login"
}

if ([string]::IsNullOrWhiteSpace($Owner)) {
    $Owner = (& gh api user --jq .login).Trim()
}

if ([string]::IsNullOrWhiteSpace($ReleaseTitle)) {
    $ReleaseTitle = "NFOX Demo $tag"
}

$fullRepo = "$Owner/$Repo"
& gh repo view $fullRepo 2>$null | Out-Null
if ($LASTEXITCODE -ne 0) {
    if (-not $AutoCreateRepository) {
        throw "Repository does not exist: $fullRepo. Re-run with -AutoCreateRepository to create it."
    }

    Push-Location $root
    try {
        & gh repo create $Repo --public --source . --remote origin --push
    }
    finally {
        Pop-Location
    }
}

if (-not $SkipBuild) {
    & (Join-Path $PSScriptRoot "build-release.ps1") -Version $Version -Owner $Owner -Repo $Repo
}

$artifactDir = Join-Path $root "artifacts\releases\$tag"
$assets = @(
    (Join-Path $artifactDir "manifest.json"),
    (Join-Path $artifactDir "NFOX.DemoApp-$Version.zip"),
    (Join-Path $artifactDir "NFOX.Migrations-$Version.zip"),
    (Join-Path $artifactDir "checksums.txt")
)

foreach ($asset in $assets) {
    if (-not (Test-Path -LiteralPath $asset)) {
        throw "Release asset was not found: $asset"
    }
}

& gh release view $tag --repo $fullRepo 2>$null | Out-Null
if ($LASTEXITCODE -eq 0) {
    & gh release edit $tag --repo $fullRepo --title $ReleaseTitle --notes "NFOX Auto Update Demo $tag"
    & gh release upload $tag --repo $fullRepo $assets --clobber
}
else {
    & gh release create $tag --repo $fullRepo --title $ReleaseTitle --notes "NFOX Auto Update Demo $tag" $assets
}

Write-Host "Release page:"
Write-Host "https://github.com/$fullRepo/releases/tag/$tag"
Write-Host "Latest manifest URL:"
Write-Host "https://github.com/$fullRepo/releases/latest/download/manifest.json"
Write-Host "Version-specific manifest URL:"
Write-Host "https://github.com/$fullRepo/releases/download/$tag/manifest.json"
Write-Host "Download URLs:"
Write-Host "https://github.com/$fullRepo/releases/download/$tag/manifest.json"
Write-Host "https://github.com/$fullRepo/releases/download/$tag/NFOX.DemoApp-$Version.zip"
Write-Host "https://github.com/$fullRepo/releases/download/$tag/NFOX.Migrations-$Version.zip"
