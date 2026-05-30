param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$Owner = "",
    [string]$Repo = "nfox-auto-update-channel",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$tag = "v$Version"
$artifactDir = Join-Path $root "artifacts\releases\$tag"
$publishRoot = Join-Path $root "artifacts\publish\$tag"
$appPublishDir = Join-Path $publishRoot "NFOX.DemoApp"
$updaterPublishDir = Join-Path $publishRoot "NFOX.DemoUpdater"
$packageName = "NFOX.UpdatePackage-$Version"
$packageStage = Join-Path $publishRoot $packageName
$packageAppDir = Join-Path $packageStage "app"
$packageUpdaterDir = Join-Path $packageStage "updater"
$packageMigrationDir = Join-Path $packageStage "migrations"
$packageManifestFile = Join-Path $packageStage "manifest.json"
$updatePackageZip = Join-Path $artifactDir "$packageName.zip"
$checksumsFile = Join-Path $artifactDir "checksums.txt"
$manifestFile = Join-Path $artifactDir "manifest.json"
$migrationSource = Join-Path $root "releases\$tag\migrations"
$manifestTemplateFile = Join-Path $root "releases\$tag\manifest.json"

function Read-JsonFile {
    param([string]$Path)
    return [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
}

function Set-JsonProperty {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        $Value
    )

    if ($Object.PSObject.Properties.Name -contains $Name) {
        $Object.$Name = $Value
    }
    else {
        $Object | Add-Member -NotePropertyName $Name -NotePropertyValue $Value
    }
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
    param([string]$FileName)
    if ([string]::IsNullOrWhiteSpace($Owner)) {
        return "PUT_GITHUB_ASSET_URL_HERE"
    }

    return "https://github.com/$Owner/$Repo/releases/download/$tag/$FileName"
}

function Set-UpdateChannelSettings {
    param($Settings)

    Set-JsonProperty -Object $Settings -Name "updateSource" -Value "GitHub"
    Set-JsonProperty -Object $Settings -Name "gitHubUseLatestRelease" -Value $true

    if (-not [string]::IsNullOrWhiteSpace($Owner)) {
        Set-JsonProperty -Object $Settings -Name "gitHubOwner" -Value $Owner
        Set-JsonProperty -Object $Settings -Name "gitHubRepo" -Value $Repo
        Set-JsonProperty -Object $Settings -Name "manifestUrl" -Value "https://github.com/$Owner/$Repo/releases/latest/download/manifest.json"
    }
}

function New-ReleaseManifest {
    param([string]$PackageHash)

    return [ordered]@{
        appName = "NFOX ERP Demo"
        updateName = $script:updateName
        latestAppVersion = $Version
        minimumRequiredAppVersion = $script:minimumRequiredAppVersion
        targetDbVersion = $script:targetDbVersion
        isRequired = $script:isRequired
        releaseNotes = $script:releaseNotes
        publishedAt = $script:publishedAt
        packages = [ordered]@{
            updatePackage = [ordered]@{
                fileName = "$packageName.zip"
                downloadUrl = Get-DownloadUrl -FileName "$packageName.zip"
                sha256 = $PackageHash
            }
        }
    }
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
Set-JsonProperty -Object $appSettings -Name "appVersion" -Value $Version
Set-JsonProperty -Object $appSettings -Name "updateName" -Value $updateName
Set-UpdateChannelSettings -Settings $appSettings
$appSettings | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $appSettingsFile -Encoding UTF8

$updaterSettingsFile = Join-Path $updaterPublishDir "appsettings.json"
$updaterSettings = Read-JsonFile -Path $updaterSettingsFile
Set-JsonProperty -Object $updaterSettings -Name "currentAppVersion" -Value $Version
Set-UpdateChannelSettings -Settings $updaterSettings
$updaterSettings | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $updaterSettingsFile -Encoding UTF8

New-Item -ItemType Directory -Path $packageAppDir, $packageUpdaterDir, $packageMigrationDir -Force | Out-Null
Copy-Item -Path (Join-Path $appPublishDir "*") -Destination $packageAppDir -Recurse -Force
Copy-Item -Path (Join-Path $updaterPublishDir "*") -Destination $packageUpdaterDir -Recurse -Force
Copy-Item -Path (Join-Path $migrationSource "*.sql") -Destination $packageMigrationDir -Force
Get-ChildItem -LiteralPath $packageStage -Filter "*.pdb" -Recurse | Remove-Item -Force

$internalManifest = New-ReleaseManifest -PackageHash "RECORDED_IN_RELEASE_MANIFEST"
$internalManifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $packageManifestFile -Encoding UTF8

Compress-Archive -Path $packageStage -DestinationPath $updatePackageZip -Force
$updatePackageHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $updatePackageZip).Hash.ToLowerInvariant()

"$updatePackageHash  $(Split-Path $updatePackageZip -Leaf)" | Set-Content -LiteralPath $checksumsFile -Encoding UTF8

$manifest = New-ReleaseManifest -PackageHash $updatePackageHash
$manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $manifestFile -Encoding UTF8

Write-Host "Release artifacts created in $artifactDir"
Write-Host "Update package: $updatePackageZip"
Write-Host "Checksums: $checksumsFile"
Write-Host "Manifest: $manifestFile"
