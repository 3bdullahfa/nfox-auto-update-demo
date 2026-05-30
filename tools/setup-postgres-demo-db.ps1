param(
    [string]$HostName = "localhost",
    [int]$Port = 5432,
    [string]$User = "postgres",
    [string]$Password = "postgres",
    [string]$Database = "nfox_demo"
)

$ErrorActionPreference = "Stop"

$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$sqlDir = Join-Path $root "database\postgres"
$scripts = @(
    "001_create_initial_schema.sql",
    "002_seed_initial_data.sql",
    "003_insert_initial_schema_version.sql"
)

if (-not (Get-Command psql -ErrorAction SilentlyContinue)) {
    throw "psql is not installed or is not available in PATH. Install PostgreSQL client tools first."
}

if ($Database -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
    throw "Invalid database name '$Database'. Use letters, digits, and underscores only, starting with a letter or underscore."
}

$oldPassword = $env:PGPASSWORD
$env:PGPASSWORD = $Password

function Invoke-Psql {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DbName,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [Parameter(Mandatory = $true)]
        [string]$FailureMessage
    )

    & psql `
        -h $HostName `
        -p $Port `
        -U $User `
        -d $DbName `
        @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw $FailureMessage
    }
}

try {
    Write-Host "Checking PostgreSQL connection..." -ForegroundColor Cyan

    Invoke-Psql `
        -DbName "postgres" `
        -Arguments @("-v", "ON_ERROR_STOP=1", "-c", "SELECT version();") `
        -FailureMessage "Failed to connect to PostgreSQL server '${HostName}:$Port' as user '$User'."

    Write-Host "Checking database '$Database'..." -ForegroundColor Cyan

    $databaseLiteral = $Database.Replace("'", "''")
    $existsOutput = & psql `
        -h $HostName `
        -p $Port `
        -U $User `
        -d postgres `
        -t `
        -A `
        -c "SELECT 1 FROM pg_database WHERE datname = '$databaseLiteral';"

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to check database existence."
    }

    $existsText = ""

    if ($null -ne $existsOutput) {
        $existsText = ($existsOutput | Out-String).Trim()
    }

    if ($existsText -ne "1") {
        Write-Host "Creating database '$Database'..." -ForegroundColor Yellow

        & psql `
            -h $HostName `
            -p $Port `
            -U $User `
            -d postgres `
            -v ON_ERROR_STOP=1 `
            -c "CREATE DATABASE $Database;"

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create database '$Database'."
        }
    }
    else {
        Write-Host "Database '$Database' already exists." -ForegroundColor Green
    }

    Write-Host "Running PostgreSQL setup scripts..." -ForegroundColor Cyan

    foreach ($script in $scripts) {
        $scriptPath = Join-Path $sqlDir $script

        if (-not (Test-Path -LiteralPath $scriptPath)) {
            throw "Missing SQL script: $scriptPath"
        }

        Write-Host "Running $script..." -ForegroundColor Yellow

        Invoke-Psql `
            -DbName $Database `
            -Arguments @("-v", "ON_ERROR_STOP=1", "-f", $scriptPath) `
            -FailureMessage "Failed while running SQL script '$scriptPath'."
    }

    Write-Host ""
    Write-Host "PostgreSQL demo database is ready." -ForegroundColor Green
    Write-Host "Connection string:" -ForegroundColor Cyan
    Write-Host "Host=$HostName;Port=$Port;Database=$Database;Username=$User;Password=$Password"
}
finally {
    $env:PGPASSWORD = $oldPassword
}
