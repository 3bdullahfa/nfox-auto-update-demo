# Prompt.md — Build Complete NFOX Auto Update Demo Project

## Current Environment Note

GitHub CLI has already been installed and authenticated successfully on this Windows machine.

Use the existing authenticated `gh` session for all GitHub operations.

Before using GitHub, run:

```powershell
gh auth status
```

Then detect the current GitHub username:

```powershell
gh api user --jq .login
```

Do not request my GitHub password.
Do not request a Personal Access Token unless `gh auth status` fails.
Prefer GitHub CLI commands over manual token-based API calls.

The default GitHub repository name for this proof of concept is:

```text
nfox-auto-update-demo
```

---

# Role

You are a senior .NET Windows Desktop engineer and DevOps engineer.

Build a complete working proof-of-concept project that demonstrates automatic application updates and automatic database schema migrations for a Windows Forms accounting/ERP-style application.

The project name is:

```text
NFOX Auto Update Demo
```

The goal is to simulate how our real NFOX ERP system will update itself over the internet and update the database structure automatically without manually replacing files.

---

# Main Objective

Build a complete C# Windows Forms solution that contains:

1. A demo Windows Forms application.
2. A separate updater Windows Forms application.
3. A shared library for common models, services, database logic, and migration logic.
4. A database migration engine.
5. Local database setup scripts.
6. GitHub Releases integration for update packages.
7. Build and packaging scripts that create versioned releases.
8. Documentation explaining how to run the demo and publish test updates.

The final result must allow this scenario:

1. Install/run version `1.0.0`.
2. The app displays:

   * App version.
   * Update name.
   * Database provider.
   * Database version.
   * Customer data from the database.
3. Publish version `1.0.1` as a GitHub Release.
4. The updater checks GitHub for a new version.
5. The updater downloads the new package.
6. The updater applies database migrations.
7. The updater replaces the application files.
8. The updated app runs and displays the new version and updated database structure/data.

---

# Technology Requirements

Use:

* C#
* .NET 8 or latest stable .NET LTS available on the machine
* Windows Forms
* ADO.NET
* PostgreSQL as the default demo database
* `Npgsql` for PostgreSQL access
* Optional provider structure for SQL Server and Oracle later
* GitHub Releases for test update hosting
* JSON manifest files
* ZIP packages
* SHA256 verification
* PowerShell scripts for build and release automation

Do not use WPF.
Do not use paid libraries.
Do not build a web application.
Do not skip the updater.
Do not merge the updater into the main app.
The updater must be a separate executable.

---

# Preferred Database for the Demo

Use PostgreSQL as the first working implementation because it is simple for testing.

However, design the database layer so that the future addition of SQL Server or Oracle is straightforward.

Create an interface similar to:

```csharp
public interface IDatabaseProvider
{
    string ProviderName { get; }
    IDbConnection CreateConnection(string connectionString);
}
```

Create PostgreSQL implementation first:

```text
PostgresDatabaseProvider
```

Optional placeholders are acceptable for:

```text
SqlServerDatabaseProvider
OracleDatabaseProvider
```

But the PostgreSQL flow must fully work.

---

# Required Solution Structure

Create this solution structure:

```text
NFOX.AutoUpdateDemo/
  README.md
  Prompt.md
  .gitignore
  NFOX.AutoUpdateDemo.sln

  src/
    NFOX.DemoApp/
      NFOX.DemoApp.csproj
      Program.cs
      MainForm.cs
      MainForm.Designer.cs
      appsettings.json

    NFOX.DemoUpdater/
      NFOX.DemoUpdater.csproj
      Program.cs
      UpdaterForm.cs
      UpdaterForm.Designer.cs
      appsettings.json

    NFOX.Shared/
      NFOX.Shared.csproj
      Models/
        UpdateManifest.cs
        PackageInfo.cs
        AppConfig.cs
        UpdaterConfig.cs
        MigrationInfo.cs
      Services/
        VersionService.cs
        FileHashService.cs
        ZipService.cs
        LogService.cs
        DownloadService.cs
        ConfigService.cs
        BackupService.cs
        ProcessService.cs
      Database/
        IDatabaseProvider.cs
        PostgresDatabaseProvider.cs
        SqlServerDatabaseProvider.cs
        OracleDatabaseProvider.cs
        DatabaseProviderFactory.cs
        DatabaseService.cs
        MigrationRunner.cs

  database/
    postgres/
      001_create_initial_schema.sql
      002_seed_initial_data.sql
      003_insert_initial_schema_version.sql

  releases/
    v1.0.0/
      manifest.json
      migrations/
        2026.05.30.001__initial_schema.sql

    v1.0.1/
      manifest.json
      migrations/
        2026.05.30.002__add_tax_no_to_customers.sql

  tools/
    build-release.ps1
    publish-github-release.ps1
    create-local-release.ps1
    setup-postgres-demo-db.ps1
    clean-demo.ps1

  docs/
    update-flow.md
    github-release-setup.md
    database-migration-rules.md
    oracle-future-support.md
```

---

# Main App Requirements

The main application must be a Windows Forms app named:

```text
NFOX.DemoApp.exe
```

The main form must display:

* System name: `NFOX ERP Demo`
* App version
* Update name
* Database provider
* Database version
* Connection status
* A `DataGridView` showing customer data
* A button: `Check for Update`
* A button: `Refresh Data`
* A button: `Open Logs Folder`
* A button: `Exit`

The `Check for Update` button should launch:

```text
NFOX.DemoUpdater.exe
```

The main app must read its local config from:

```json
{
  "AppName": "NFOX ERP Demo",
  "AppVersion": "1.0.0",
  "UpdateName": "Initial Release",
  "DatabaseProvider": "PostgreSQL",
  "ConnectionString": "Host=localhost;Port=5432;Database=nfox_demo;Username=postgres;Password=postgres",
  "UpdaterPath": "../NFOX.DemoUpdater/NFOX.DemoUpdater.exe"
}
```

When the app starts, it must:

1. Load `appsettings.json`.
2. Connect to the database.
3. Ensure the migration/version table exists.
4. Read the current database version.
5. Load customer data.
6. Show all relevant values on the screen.
7. Write startup and database status to logs.

The app must handle Arabic text correctly. The update name may be Arabic.

---

# Updater App Requirements

The updater must be a separate Windows Forms executable named:

```text
NFOX.DemoUpdater.exe
```

The updater form must show:

* Current app version
* Latest app version from server
* Current DB version
* Target DB version
* Update name
* Release notes
* Download progress
* Migration progress
* Status log textbox
* Button: `Check`
* Button: `Download and Update`
* Button: `Cancel`
* Button: `Open Logs Folder`
* Button: `Close`

The updater must read config from:

```json
{
  "AppName": "NFOX ERP Demo",
  "CurrentAppVersion": "1.0.0",
  "ManifestUrl": "PUT_GITHUB_RELEASE_MANIFEST_URL_HERE",
  "InstallDirectory": "../NFOX.DemoApp",
  "BackupDirectory": "../../backups",
  "DownloadDirectory": "../../downloads",
  "DatabaseProvider": "PostgreSQL",
  "ConnectionString": "Host=localhost;Port=5432;Database=nfox_demo;Username=postgres;Password=postgres"
}
```

The updater must support both remote and local manifest URLs:

```text
https://github.com/.../manifest.json
```

and:

```text
file:///C:/path/to/manifest.json
```

This allows testing the full update flow locally before using GitHub.

---

# Demo Database Schema

Create an initial database schema for PostgreSQL.

Initial version:

```text
2026.05.30.001
```

Create this table:

```sql
CREATE TABLE IF NOT EXISTS customers (
    id INT PRIMARY KEY,
    customer_name VARCHAR(200) NOT NULL,
    balance NUMERIC(18,2) NOT NULL DEFAULT 0
);
```

Seed data:

```sql
INSERT INTO customers (id, customer_name, balance)
VALUES
(1, 'Customer 1', 1500),
(2, 'Customer 2', 2750),
(3, 'Customer 3', 900)
ON CONFLICT (id) DO NOTHING;
```

Create a database migration/version table:

```sql
CREATE TABLE IF NOT EXISTS nfox_schema_version (
    id BIGSERIAL PRIMARY KEY,
    version_no VARCHAR(50) NOT NULL,
    script_name VARCHAR(300) NOT NULL,
    checksum VARCHAR(100),
    applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(20) NOT NULL,
    error_message TEXT,
    machine_name VARCHAR(200),
    app_version VARCHAR(50)
);
```

Create a database update lock table:

```sql
CREATE TABLE IF NOT EXISTS nfox_update_lock (
    lock_id INT PRIMARY KEY,
    machine_name VARCHAR(200),
    locked_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

Insert initial schema version if it does not already exist:

```sql
INSERT INTO nfox_schema_version
(version_no, script_name, checksum, status, machine_name, app_version)
SELECT
'2026.05.30.001',
'2026.05.30.001__initial_schema.sql',
NULL,
'SUCCESS',
'INITIAL_SETUP',
'1.0.0'
WHERE NOT EXISTS (
    SELECT 1
    FROM nfox_schema_version
    WHERE version_no = '2026.05.30.001'
      AND script_name = '2026.05.30.001__initial_schema.sql'
      AND status = 'SUCCESS'
);
```

---

# Version 1.0.1 Database Migration

Create a migration file:

```text
2026.05.30.002__add_tax_no_to_customers.sql
```

For PostgreSQL, it should safely add a new column:

```sql
ALTER TABLE customers
ADD COLUMN IF NOT EXISTS tax_no VARCHAR(50);

UPDATE customers
SET tax_no = 'TAX-' || id
WHERE tax_no IS NULL;
```

After this update, the app should display the new `tax_no` column automatically in the `DataGridView`.

---

# Database Provider Design

Create a database provider design that allows this later:

```json
{
  "DatabaseProvider": "Oracle",
  "ConnectionString": "User Id=...;Password=...;Data Source=..."
}
```

The PostgreSQL implementation must work.

Create provider classes:

```text
IDatabaseProvider
PostgresDatabaseProvider
SqlServerDatabaseProvider
OracleDatabaseProvider
DatabaseProviderFactory
```

For now:

* `PostgresDatabaseProvider` must be fully implemented.
* `SqlServerDatabaseProvider` may throw `NotImplementedException` with a clear message.
* `OracleDatabaseProvider` may throw `NotImplementedException` with a clear message.

Document future Oracle support.

Mention that Oracle migration scripts need Oracle-specific SQL syntax, for example:

```sql
ALTER TABLE CUSTOMERS ADD TAX_NO VARCHAR2(50);
```

For PostgreSQL:

```sql
ALTER TABLE customers ADD COLUMN IF NOT EXISTS tax_no VARCHAR(50);
```

For SQL Server:

```sql
IF COL_LENGTH('customers', 'tax_no') IS NULL
BEGIN
    ALTER TABLE customers ADD tax_no VARCHAR(50);
END
```

---

# Migration Engine Requirements

Implement a robust `MigrationRunner`.

It must:

1. Read `.sql` files from a migrations folder.
2. Sort them by filename.
3. Extract the version number from the filename.
4. Compute SHA256 checksum for each SQL file.
5. Check `nfox_schema_version` before running a script.
6. Skip scripts already applied successfully.
7. Execute each pending script inside a database transaction where possible.
8. Insert a success record after each successful migration.
9. Insert a failed record if migration fails.
10. Stop the update process on migration failure.
11. Log all operations to a local log file.
12. Return clear results to the updater UI.

Use this filename format:

```text
YYYY.MM.DD.NNN__description.sql
```

Example:

```text
2026.05.30.002__add_tax_no_to_customers.sql
```

Do not execute the same migration twice if it already exists as `SUCCESS`.

---

# Database Locking Requirement

Before applying migrations, acquire an update lock using `nfox_update_lock`.

Use a simple approach for the proof of concept:

1. Try inserting or updating lock row with `lock_id = 1`.
2. If lock is already held recently by another machine, stop the migration.
3. Release the lock when done.
4. Log lock acquisition and release.

For production, this can later be replaced with database-native advisory locks.

---

# Update Manifest Format

Use this manifest structure:

```json
{
  "appName": "NFOX ERP Demo",
  "updateName": "إضافة الرقم الضريبي للعملاء",
  "latestAppVersion": "1.0.1",
  "minimumRequiredAppVersion": "1.0.0",
  "targetDbVersion": "2026.05.30.002",
  "isRequired": false,
  "releaseNotes": "Adds tax_no column to customers and displays it in the customer grid.",
  "publishedAt": "2026-05-30T00:00:00Z",
  "packages": {
    "app": {
      "fileName": "NFOX.DemoApp-1.0.1.zip",
      "downloadUrl": "PUT_GITHUB_ASSET_URL_HERE",
      "sha256": "TO_BE_GENERATED_BY_BUILD_SCRIPT"
    },
    "migrations": {
      "fileName": "NFOX.Migrations-1.0.1.zip",
      "downloadUrl": "PUT_GITHUB_ASSET_URL_HERE",
      "sha256": "TO_BE_GENERATED_BY_BUILD_SCRIPT"
    }
  }
}
```

Create C# model classes for this manifest:

```text
UpdateManifest
PackageInfo
UpdatePackages
```

---

# Update Process Requirements

The updater must perform this sequence:

1. Load local config.
2. Load current local app version.
3. Connect to database.
4. Read current database version.
5. Download remote or local `manifest.json`.
6. Compare local app version with `latestAppVersion`.
7. If no update exists, display `No update available`.
8. If an update exists:

   * Display update details.
   * Download app package.
   * Download migration package.
   * Verify SHA256 hashes.
   * Create backup of current app folder.
   * Extract migrations to temporary folder.
   * Apply pending migrations.
   * If migrations succeed:

     * Extract new app package to install directory.
     * Update local app config with the new version and update name.
     * Preserve the existing database connection string.
     * Launch the updated app.
   * If migrations fail:

     * Do not replace app files.
     * Show error.
     * Keep logs.
     * Keep backup.
9. Never delete the backup automatically.

Important safety rule:

```text
Do not replace the application files until database migrations succeed.
```

---

# File Replacement Rules

When replacing application files:

1. Ensure `NFOX.DemoApp.exe` is not running.
2. Backup current install directory first.
3. Extract new files to a temporary folder.
4. Copy from temp folder to install directory.
5. Keep updater files separate from demo app files.
6. Do not delete user config unless the release intentionally contains a config migration.
7. For this demo, preserve the database connection string during app config update.

If the app is still running and files cannot be replaced, show a clear message asking the user to close the app.

---

# GitHub Integration Requirements

GitHub CLI is already installed and authenticated on this machine using:

```powershell
gh auth login
```

Do not ask for a GitHub password.
Do not ask for a Personal Access Token unless GitHub CLI authentication fails.
Do not store any GitHub credentials in source code, config files, manifests, scripts, or logs.

Before doing any GitHub operation, verify authentication by running:

```powershell
gh auth status
```

Also detect the current GitHub username automatically by running:

```powershell
gh api user --jq .login
```

Use that username as the default GitHub repository owner unless I explicitly provide another owner or organization.

---

# GitHub Repository Setup

Use this repository name by default:

```text
nfox-auto-update-demo
```

Check whether the repository already exists:

```powershell
gh repo view <OWNER>/nfox-auto-update-demo
```

If it does not exist, create it.

For this proof of concept, prefer creating it as a public repository because the demo updater needs to download `manifest.json`, app ZIP, and migrations ZIP without embedding GitHub credentials inside the client app.

Use:

```powershell
gh repo create nfox-auto-update-demo --public --source . --remote origin --push
```

If the repository already exists, do not recreate it. Instead, ensure the local repository is connected to the correct remote:

```powershell
git remote -v
```

If needed, set the remote:

```powershell
git remote add origin https://github.com/<OWNER>/nfox-auto-update-demo.git
```

or update it:

```powershell
git remote set-url origin https://github.com/<OWNER>/nfox-auto-update-demo.git
```

---

# GitHub Releases Publishing

Use GitHub Releases as the test update server.

The release publishing script must use the already-authenticated GitHub CLI session.

Create this script:

```text
tools/publish-github-release.ps1
```

It must support:

```powershell
.\tools\publish-github-release.ps1 `
  -Owner "<AUTO_DETECTED_OR_PROVIDED_OWNER>" `
  -Repo "nfox-auto-update-demo" `
  -Version "1.0.1" `
  -ReleaseTitle "NFOX Demo v1.0.1"
```

If `-Owner` is not provided, auto-detect it using:

```powershell
gh api user --jq .login
```

The script must:

1. Check that `gh` is installed.
2. Check that `gh auth status` succeeds.
3. Check that the repository exists.
4. Create the repository if it does not exist and the user requested auto-create mode.
5. Build or locate release artifacts from:

```text
artifacts/releases/v1.0.1/
```

6. Create or update GitHub Release:

```text
v1.0.1
```

7. Upload these assets:

```text
manifest.json
NFOX.DemoApp-1.0.1.zip
NFOX.Migrations-1.0.1.zip
checksums.txt
```

8. If an asset already exists, delete and re-upload it or use a safe overwrite strategy.
9. Print the final release page URL.
10. Print the downloadable URLs needed by the updater.

---

# GitHub Release Asset URL Handling

The updater must be able to download the update files over HTTPS.

For a public repository, release assets can be downloaded without credentials using browser-download URLs.

The manifest should contain direct download URLs similar to:

```text
https://github.com/<OWNER>/nfox-auto-update-demo/releases/download/v1.0.1/NFOX.DemoApp-1.0.1.zip
https://github.com/<OWNER>/nfox-auto-update-demo/releases/download/v1.0.1/NFOX.Migrations-1.0.1.zip
```

The manifest itself may also be uploaded as a release asset and referenced as:

```text
https://github.com/<OWNER>/nfox-auto-update-demo/releases/download/v1.0.1/manifest.json
```

The updater config should allow setting:

```json
{
  "ManifestUrl": "https://github.com/<OWNER>/nfox-auto-update-demo/releases/download/v1.0.1/manifest.json"
}
```

---

# Important Private Repository Note

If the GitHub repository is private, the updater will not be able to download release assets without authentication.

For this proof of concept, use a public repository and do not include any real company files, customer data, credentials, or proprietary binaries.

For production, do not use public GitHub Releases for real NFOX ERP updates. Use a private update server, Azure Blob Storage with signed URLs, S3-compatible storage with signed URLs, or a protected API.

---

# Required Script Behavior

Update `tools/publish-github-release.ps1` so that it does not require a token by default.

It should use:

```powershell
gh auth status
gh release create
gh release upload
gh release view
```

The script should fail with a clear message if GitHub CLI is not authenticated:

```text
GitHub CLI is not authenticated. Please run: gh auth login
```

Do not ask for `GITHUB_TOKEN` unless `gh auth status` fails.

---

# Repository Commit and Push

After creating the full project, initialize git if needed:

```powershell
git init
git add .
git commit -m "Initial NFOX auto update demo"
```

If the remote is configured, push:

```powershell
git branch -M main
git push -u origin main
```

Do not push secrets, database passwords beyond local demo defaults, tokens, build outputs that are not intended to be versioned, or local logs.

Add a `.gitignore` that excludes:

```text
bin/
obj/
.vs/
logs/
downloads/
backups/
artifacts/
*.user
*.suo
.env
```

Release artifacts should be generated locally under `artifacts/`, but not committed unless explicitly requested.

---

# Build Scripts

Create:

```text
tools/build-release.ps1
```

It must:

1. Accept a version parameter.
2. Build the solution in Release mode.
3. Publish the app.
4. Publish the updater.
5. Create ZIP packages.
6. Copy migrations for the selected version.
7. Compute SHA256 hashes.
8. Generate a draft `manifest.json`.
9. Place all artifacts in:

```text
artifacts/releases/vX.Y.Z/
```

Expected output for version `1.0.1`:

```text
artifacts/releases/v1.0.1/
  manifest.json
  NFOX.DemoApp-1.0.1.zip
  NFOX.Migrations-1.0.1.zip
  checksums.txt
```

The script should support parameters similar to:

```powershell
.\tools\build-release.ps1 -Version "1.0.1" -Owner "<OWNER>" -Repo "nfox-auto-update-demo"
```

The generated manifest should include GitHub download URLs when `Owner` and `Repo` are provided.

---

# Local Release Script

Create:

```text
tools/create-local-release.ps1
```

This should simulate GitHub release files locally for testing without internet.

It should create a local manifest with `file:///` URLs pointing to the local ZIP packages.

Expected local output:

```text
artifacts/local-release/v1.0.1/
  manifest.json
  NFOX.DemoApp-1.0.1.zip
  NFOX.Migrations-1.0.1.zip
  checksums.txt
```

The updater must be able to consume this local manifest.

---

# PostgreSQL Setup Script

Create:

```text
tools/setup-postgres-demo-db.ps1
```

It should:

1. Create database `nfox_demo` if possible.
2. Run scripts from:

```text
database/postgres/
```

3. Print connection instructions.
4. Fail clearly if `psql` is not installed or PostgreSQL is not reachable.

Default database connection:

```text
Host=localhost;Port=5432;Database=nfox_demo;Username=postgres;Password=postgres
```

Do not assume the user has the password `postgres`; allow a parameter:

```powershell
.\tools\setup-postgres-demo-db.ps1 -User "postgres" -Password "postgres"
```

---

# Logging Requirements

Create logs in:

```text
logs/
```

Log files should be named like:

```text
updater-2026-05-30.log
app-2026-05-30.log
migration-2026-05-30.log
```

Logs must include:

* Timestamp
* Level
* Operation
* Message
* Exception details if any

Do not silently swallow exceptions.

---

# README Requirements

Create a detailed `README.md` containing:

1. Project purpose.
2. Architecture diagram in text form.
3. Prerequisites.
4. PostgreSQL setup.
5. How to create the demo database.
6. How to run version `1.0.0`.
7. How to build version `1.0.1`.
8. How to test update locally.
9. How to publish update to GitHub Releases.
10. How to configure the updater manifest URL.
11. Troubleshooting.
12. Notes for future Oracle 12c support.
13. Security notes about not using public GitHub Releases for real production ERP updates.

---

# Required Documentation Files

Create:

```text
docs/update-flow.md
```

Explain this flow:

```text
DemoApp -> Updater -> Manifest -> Download -> Verify -> Migrate DB -> Replace Files -> Launch App
```

Create:

```text
docs/github-release-setup.md
```

Explain GitHub test release setup using authenticated GitHub CLI.

Create:

```text
docs/database-migration-rules.md
```

Explain:

* Migration naming.
* Ordering.
* Idempotency.
* Checksums.
* History table.
* Failed migration behavior.
* Rollback considerations.

Create:

```text
docs/oracle-future-support.md
```

Explain how the same architecture can later support Oracle 12c, including:

* Oracle connection string.
* Oracle-specific SQL syntax.
* Oracle transaction limitations with DDL.
* Need to test scripts carefully in Oracle.
* Recommended use of safe/idempotent PL/SQL blocks.

---

# UI Language

Use English labels in code for simplicity, but make the UI friendly and allow Arabic values.

The sample update name must be:

```text
إضافة الرقم الضريبي للعملاء
```

The app must handle UTF-8/Arabic text correctly.

---

# Error Handling Requirements

Handle these cases clearly:

1. No internet.
2. Invalid manifest URL.
3. GitHub asset not found.
4. SHA256 mismatch.
5. Database connection failed.
6. Migration failed.
7. App is still running and files cannot be replaced.
8. Permission denied while copying files.
9. Invalid version format.
10. Missing config file.
11. Missing PostgreSQL driver/package.
12. Missing GitHub CLI in publishing script.
13. GitHub CLI not authenticated.
14. Repository does not exist.
15. Release asset already exists.

Show clear messages in the updater UI and write details to logs.

---

# Security Requirements

Do not store tokens or passwords in source code.

Do not commit:

```text
.env
logs/
downloads/
backups/
artifacts/
```

Use the demo PostgreSQL password only as a local default for development. Make it configurable.

Do not include real company binaries, customer data, or production credentials.

For the demo, public GitHub Releases are acceptable.

For production, document that a private update server or signed URLs are required.

---

# Acceptance Criteria

The project is complete only when all these work.

## Initial Run

* I can run the database setup script.
* I can run `NFOX.DemoApp`.
* It displays version `1.0.0`.
* It displays update name `Initial Release`.
* It displays customer rows from PostgreSQL.
* It displays DB version `2026.05.30.001`.

## Local Update Test

* I can build release `1.0.1`.
* I can point updater to a local `manifest.json`.
* Updater detects new version.
* Updater reads or downloads local packages.
* Updater verifies SHA256 hashes.
* Updater applies migration `2026.05.30.002`.
* Updater replaces the app files.
* Updated app opens.
* App displays version `1.0.1`.
* App displays update name `إضافة الرقم الضريبي للعملاء`.
* Customer grid includes `tax_no`.

## GitHub CLI Test

* `gh auth status` succeeds.
* The script can detect my GitHub username using:

```powershell
gh api user --jq .login
```

## GitHub Repository Test

* The repository `nfox-auto-update-demo` exists on GitHub.
* The code is pushed to GitHub.
* No secrets are pushed.

## GitHub Release Test

* Release `v1.0.1` exists.
* Release assets exist:

  * `manifest.json`
  * `NFOX.DemoApp-1.0.1.zip`
  * `NFOX.Migrations-1.0.1.zip`
  * `checksums.txt`
* The `manifest.json` contains valid GitHub release download URLs.
* The updater can download the manifest and packages from GitHub.
* SHA256 verification passes.
* The update from `1.0.0` to `1.0.1` completes successfully.

---

# Implementation Guidance

Implement clean, simple code. Avoid over-engineering.

Use these classes or similar:

```text
NFOX.Shared.Models.UpdateManifest
NFOX.Shared.Models.PackageInfo
NFOX.Shared.Models.AppConfig
NFOX.Shared.Models.UpdaterConfig
NFOX.Shared.Models.MigrationInfo

NFOX.Shared.Services.VersionService
NFOX.Shared.Services.FileHashService
NFOX.Shared.Services.ZipService
NFOX.Shared.Services.LogService
NFOX.Shared.Services.DownloadService
NFOX.Shared.Services.ConfigService
NFOX.Shared.Services.BackupService
NFOX.Shared.Services.ProcessService

NFOX.Shared.Database.DatabaseService
NFOX.Shared.Database.MigrationRunner
NFOX.Shared.Database.IDatabaseProvider
NFOX.Shared.Database.PostgresDatabaseProvider
NFOX.Shared.Database.DatabaseProviderFactory
```

For HTTP downloads, use:

```csharp
HttpClient
```

For ZIP extraction, use:

```csharp
System.IO.Compression.ZipFile
```

For JSON, use:

```csharp
System.Text.Json
```

For PostgreSQL, use:

```text
Npgsql
```

---

# Version Comparison

Use semantic version comparison for app versions:

```text
1.0.0
1.0.1
1.1.0
2.0.0
```

Do not compare version strings using plain string comparison.

Use `System.Version` or a safe custom parser.

---

# Required Demo Versions

Create version `1.0.0`:

* App version: `1.0.0`
* Update name: `Initial Release`
* DB version: `2026.05.30.001`
* Customer columns:

  * `id`
  * `customer_name`
  * `balance`

Create version `1.0.1`:

* App version: `1.0.1`
* Update name: `إضافة الرقم الضريبي للعملاء`
* DB target version: `2026.05.30.002`
* Customer columns:

  * `id`
  * `customer_name`
  * `balance`
  * `tax_no`

---

# Deliverables

At the end, provide:

1. Complete source code.
2. Complete database scripts.
3. Complete PowerShell build/release scripts.
4. README.
5. Documentation files.
6. Example manifests for `v1.0.0` and `v1.0.1`.
7. Clear run instructions.
8. A short summary of what was implemented.
9. Any assumptions or limitations.
10. Exact commands to run the demo locally.
11. Exact commands to publish the GitHub release.

---

# Final Instructions

This is a proof of concept, not the final production updater.

However, the architecture should be close to production standards:

* Separate updater executable.
* Versioned app packages.
* Versioned database migrations.
* Migration history table.
* Migration lock table.
* SHA256 package verification.
* Logs.
* Backups before replacement.
* GitHub Releases for test hosting.
* No hard-coded GitHub credentials.
* No destructive database operations.
* No repeated execution of already-applied migrations.
* Clear error messages.
* Clear documentation.

Start by creating the full solution and all required files.

Then implement the working PostgreSQL flow.

Then add packaging scripts.

Then add local release testing.

Then add GitHub release publishing using the already-authenticated GitHub CLI session.

Do not stop after generating skeleton files. The project must be runnable and testable.
