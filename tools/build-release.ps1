param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$Owner = "",
    [string]$Repo = "nfox-auto-update-demo",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$tag = "v$Version"
$artifactDir = Join-Path $root "artifacts\releases\$tag"
$publishRoot = Join-Path $root "artifacts\publish\$tag"
$appPublishDir = Join-Path $publishRoot "NFOX.DemoApp"
$updaterPublishDir = Join-Path $publishRoot "NFOX.DemoUpdater"
$appZip = Join-Path $artifactDir "NFOX.DemoApp-$Version.zip"
$migrationZip = Join-Path $artifactDir "NFOX.Migrations-$Version.zip"
$checksumsFile = Join-Path $artifactDir "checksums.txt"
$manifestFile = Join-Path $artifactDir "manifest.json"
$migrationSource = Join-Path $root "releases\$tag\migrations"
$manifestTemplateFile = Join-Path $root "releases\$tag\manifest.json"

function Read-JsonFile {
    param([string]$Path)
    return [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
}

function Get-UpdateName {
    param([string]$ReleaseVersion)
    if ($ReleaseVersion -eq "1.0.1") {
        return -join @(
            [char]0x0625, [char]0x0636, [char]0x0627, [char]0x0641, [char]0x0629, ' ',
            [char]0x0627, [char]0x0644, [char]0x0631, [char]0x0642, [char]0x0645, ' ',
            [char]0x0627, [char]0x0644, [char]0x0636, [char]0x0631, [char]0x064A, [char]0x0628, [char]0x064A, ' ',
            [char]0x0644, [char]0x0644, [char]0x0639, [char]0x0645, [char]0x0644, [char]0x0627, [char]0x0621
        )
    }

    return "Initial Release"
}

function Get-TargetDbVersion {
    param([string]$Directory)
    $latest = Get-ChildItem -LiteralPath $Directory -Filter "*.sql" | Sort-Object Name | Select-Object -Last 1
    if (-not $latest) {
        throw "No migration scripts found in $Directory"
    }

    return ($latest.Name -split "__")[0]
}

function Get-DownloadUrl {
    param(
        [string]$FileName
    )
    if ([string]::IsNullOrWhiteSpace($Owner)) {
        return "PUT_GITHUB_ASSET_URL_HERE"
    }

    return "https://github.com/$Owner/$Repo/releases/download/$tag/$FileName"
}

if (-not (Test-Path -LiteralPath $migrationSource)) {
    throw "Migration folder not found for $tag`: $migrationSource"
}

Remove-Item -LiteralPath $artifactDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $publishRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $artifactDir, $appPublishDir, $updaterPublishDir -Force | Out-Null

Push-Location $root
try {
    dotnet restore "NFOX.AutoUpdateDemo.sln"
    dotnet publish "src\NFOX.DemoApp\NFOX.DemoApp.csproj" -c $Configuration -o $appPublishDir --no-restore
    dotnet publish "src\NFOX.DemoUpdater\NFOX.DemoUpdater.csproj" -c $Configuration -o $updaterPublishDir --no-restore
}
finally {
    Pop-Location
}

$updateName = Get-UpdateName -ReleaseVersion $Version
$targetDbVersion = Get-TargetDbVersion -Directory $migrationSource
$releaseNotes = if ($Version -eq "1.0.1") { "Adds tax_no column to customers and displays it in the customer grid." } else { "Initial demo release with customer table and migration history." }
$minimumRequiredAppVersion = "1.0.0"
$isRequired = $false
$publishedAt = "2026-05-30T00:00:00Z"
if (Test-Path -LiteralPath $manifestTemplateFile) {
    $template = Read-JsonFile -Path $manifestTemplateFile
    if ($template.updateName) { $updateName = $template.updateName }
    if ($template.releaseNotes) { $releaseNotes = $template.releaseNotes }
    if ($template.minimumRequiredAppVersion) { $minimumRequiredAppVersion = $template.minimumRequiredAppVersion }
    if ($null -ne $template.isRequired) { $isRequired = [bool]$template.isRequired }
    if ($template.publishedAt) { $publishedAt = $template.publishedAt }
}
$appSettingsFile = Join-Path $appPublishDir "appsettings.json"
$appSettings = Read-JsonFile -Path $appSettingsFile
$appSettings.appVersion = $Version
$appSettings.updateName = $updateName
$appSettings | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $appSettingsFile -Encoding UTF8

$updaterSettingsFile = Join-Path $updaterPublishDir "appsettings.json"
$updaterSettings = Read-JsonFile -Path $updaterSettingsFile
$updaterSettings.currentAppVersion = $Version
$updaterSettings | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $updaterSettingsFile -Encoding UTF8

Compress-Archive -Path (Join-Path $appPublishDir "*") -DestinationPath $appZip -Force
$migrationStage = Join-Path $publishRoot "migrations"
New-Item -ItemType Directory -Path $migrationStage -Force | Out-Null
Copy-Item -Path (Join-Path $migrationSource "*.sql") -Destination $migrationStage -Force
Compress-Archive -Path (Join-Path $migrationStage "*") -DestinationPath $migrationZip -Force

$appHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $appZip).Hash.ToLowerInvariant()
$migrationHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $migrationZip).Hash.ToLowerInvariant()

@(
    "$appHash  $(Split-Path $appZip -Leaf)",
    "$migrationHash  $(Split-Path $migrationZip -Leaf)"
) | Set-Content -LiteralPath $checksumsFile -Encoding UTF8

$manifest = [ordered]@{
    appName = "NFOX ERP Demo"
    updateName = $updateName
    latestAppVersion = $Version
    minimumRequiredAppVersion = $minimumRequiredAppVersion
    targetDbVersion = $targetDbVersion
    isRequired = $isRequired
    releaseNotes = $releaseNotes
    publishedAt = $publishedAt
    packages = [ordered]@{
        app = [ordered]@{
            fileName = "NFOX.DemoApp-$Version.zip"
            downloadUrl = Get-DownloadUrl -FileName "NFOX.DemoApp-$Version.zip"
            sha256 = $appHash
        }
        migrations = [ordered]@{
            fileName = "NFOX.Migrations-$Version.zip"
            downloadUrl = Get-DownloadUrl -FileName "NFOX.Migrations-$Version.zip"
            sha256 = $migrationHash
        }
    }
}

$manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $manifestFile -Encoding UTF8

Write-Host "Release artifacts created in $artifactDir"
Write-Host "App package: $appZip"
Write-Host "Migration package: $migrationZip"
Write-Host "Manifest: $manifestFile"
