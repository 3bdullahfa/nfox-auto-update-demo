param(
    [string]$Owner = "",
    [string]$Repo = "nfox-auto-update-channel",
    [Parameter(Mandatory = $true)]
    [string]$Version,
    [string]$ReleaseTitle = "",
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

function Invoke-Gh {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    & gh @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub CLI command failed: gh $($Arguments -join ' ')"
    }
}

function Test-GhCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    try {
        & gh @Arguments 2>$null | Out-Null
        return $LASTEXITCODE -eq 0
    }
    catch {
        return $false
    }
}

function Ensure-MinimalReadme {
    param([string]$FullRepo)

    if (Test-GhCommand -Arguments @("api", "repos/$FullRepo/contents/README.md")) {
        return
    }

    $readme = @"
NFOX update distribution channel.
This repository does not contain source code.
It only hosts release assets for the demo updater.
"@
    $encoded = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($readme))
    $bodyFile = Join-Path ([System.IO.Path]::GetTempPath()) "nfox-update-channel-readme-$([System.Guid]::NewGuid()).json"

    try {
        $body = @{
            message = "Add distribution channel README"
            content = $encoded
        } | ConvertTo-Json -Depth 5
        $utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false
        [System.IO.File]::WriteAllText($bodyFile, $body, $utf8NoBom)

        & gh api --method PUT "repos/$FullRepo/contents/README.md" --input $bodyFile | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "GitHub CLI command failed: gh api --method PUT repos/$FullRepo/contents/README.md --input $bodyFile"
        }
    }
    finally {
        Remove-Item -LiteralPath $bodyFile -Force -ErrorAction SilentlyContinue
    }
}

Require-Command -Name "gh"

& gh auth status | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not authenticated. Please run: gh auth login"
}

if ([string]::IsNullOrWhiteSpace($Owner)) {
    $Owner = (& gh api user --jq .login).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($Owner)) {
        throw "Failed to detect GitHub owner from the authenticated GitHub CLI session."
    }
}

if ([string]::IsNullOrWhiteSpace($ReleaseTitle)) {
    $ReleaseTitle = "NFOX Demo $tag"
}

$fullRepo = "$Owner/$Repo"
if (-not (Test-GhCommand -Arguments @("repo", "view", $fullRepo))) {
    Invoke-Gh -Arguments @("repo", "create", $fullRepo, "--public", "--description", "NFOX update distribution channel. Releases-only demo update feed.")
}

Ensure-MinimalReadme -FullRepo $fullRepo

if (-not $SkipBuild) {
    & (Join-Path $PSScriptRoot "build-release.ps1") -Version $Version -Owner $Owner -Repo $Repo
    if ($LASTEXITCODE -ne 0) {
        throw "Release build failed."
    }
}

$artifactDir = Join-Path $root "artifacts\releases\$tag"
$assets = @(
    (Join-Path $artifactDir "manifest.json"),
    (Join-Path $artifactDir "NFOX.UpdatePackage-$Version.zip"),
    (Join-Path $artifactDir "checksums.txt")
)

foreach ($asset in $assets) {
    if (-not (Test-Path -LiteralPath $asset)) {
        throw "Release asset was not found: $asset"
    }
}

if (Test-GhCommand -Arguments @("release", "view", $tag, "--repo", $fullRepo)) {
    Invoke-Gh -Arguments @("release", "edit", $tag, "--repo", $fullRepo, "--title", $ReleaseTitle, "--notes", "NFOX Auto Update Demo $tag")
    Invoke-Gh -Arguments (@("release", "upload", $tag, "--repo", $fullRepo) + $assets + @("--clobber"))
}
else {
    Invoke-Gh -Arguments (@("release", "create", $tag, "--repo", $fullRepo, "--title", $ReleaseTitle, "--notes", "NFOX Auto Update Demo $tag") + $assets)
}

Write-Host "Release page:"
Write-Host "https://github.com/$fullRepo/releases/tag/$tag"
Write-Host "Latest manifest URL:"
Write-Host "https://github.com/$fullRepo/releases/latest/download/manifest.json"
Write-Host "Version-specific manifest URL:"
Write-Host "https://github.com/$fullRepo/releases/download/$tag/manifest.json"
