param(
    [string]$HostName = "localhost",
    [int]$Port = 5432,
    [string]$Database = "nfox_demo",
    [string]$User = "postgres",
    [string]$Password = "postgres"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$scriptDir = Join-Path $root "database\postgres"

if (-not (Get-Command psql -ErrorAction SilentlyContinue)) {
    throw "psql is not installed or is not available in PATH. Install PostgreSQL client tools first."
}

$oldPassword = $env:PGPASSWORD
$env:PGPASSWORD = $Password
try {
    $escapedDatabase = $Database.Replace("'", "''")
    $createSql = "SELECT 'CREATE DATABASE $escapedDatabase' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$escapedDatabase')\gexec"
    & psql -h $HostName -p $Port -U $User -d postgres -v ON_ERROR_STOP=1 -c $createSql
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create or verify database $Database."
    }

    Get-ChildItem -LiteralPath $scriptDir -Filter "*.sql" | Sort-Object Name | ForEach-Object {
        Write-Host "Running $($_.Name)"
        & psql -h $HostName -p $Port -U $User -d $Database -v ON_ERROR_STOP=1 -f $_.FullName
        if ($LASTEXITCODE -ne 0) {
            throw "Failed while running $($_.FullName)"
        }
    }
}
finally {
    $env:PGPASSWORD = $oldPassword
}

Write-Host "PostgreSQL demo database is ready."
Write-Host "Connection string:"
Write-Host "Host=$HostName;Port=$Port;Database=$Database;Username=$User;Password=$Password"
