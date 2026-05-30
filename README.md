# NFOX Auto Update Demo

Proof of concept for a Windows Forms accounting/ERP-style application that updates application files and PostgreSQL schema over a versioned update channel.

## Architecture

```text
NFOX.DemoApp
  -> checks GitHub Releases automatically on startup
  -> shows an in-app update notification when a newer version exists
  -> launches NFOX.DemoUpdater when the user clicks تحديث الآن
      -> discovers the latest GitHub release
      -> downloads manifest.json from GitHub Releases
      -> downloads ZIP packages
      -> verifies SHA256
      -> backs up current app folder
      -> applies PostgreSQL migrations
      -> replaces app files
      -> launches updated NFOX.DemoApp

NFOX.Shared
  -> config, logging, downloads, ZIP, hashing, database providers, migrations
```

## Prerequisites

- Windows 10 or later.
- .NET 8 SDK.
- PostgreSQL server and `psql` client tools.
- Git.
- GitHub CLI authenticated with `gh auth login` for publishing GitHub Releases.

Verify GitHub CLI:

```powershell
gh auth status
gh api user --jq .login
```

## Create Demo Database

Default connection string:

```text
Host=localhost;Port=5432;Database=nfox_demo;Username=postgres;Password=postgres
```

Create and seed the database:

```powershell
.\tools\setup-postgres-demo-db.ps1 -User "postgres" -Password "postgres"
```

The setup script creates:

- `customers`
- `nfox_schema_version`
- `nfox_update_lock`

## Run Version 1.0.0

```powershell
dotnet run --project .\src\NFOX.DemoApp\NFOX.DemoApp.csproj
```

Expected values:

- App version: `1.0.0`
- Update name: `Initial Release`
- Database version: `2026.05.30.001`
- Customer rows: 3
- The app checks GitHub automatically and shows an update notification when a newer release is available.

## GitHub Automatic Update Flow

GitHub is the default update channel for this proof of concept:

```text
Owner: 3bdullahfa
Repo: nfox-auto-update-demo
Latest manifest: https://github.com/3bdullahfa/nfox-auto-update-demo/releases/latest/download/manifest.json
```

`NFOX.DemoApp` reads its local version from `appsettings.json`, checks GitHub Releases on startup, downloads the latest `manifest.json`, and compares versions using semantic version comparison.

If no update exists, the app shows:

```text
النظام محدث
```

If an update exists, the main app shows an update panel with:

- Current version.
- New version.
- Update name.
- Release notes.
- Target database version.
- Whether the update is required.

The user starts the update from the main app by clicking:

```text
تحديث الآن
```

The updater then downloads packages from GitHub Releases, verifies SHA256, applies database migrations, replaces files only after migrations succeed, and relaunches the updated app.

Latest demo update:

```text
Version: 1.0.2
Update: إضافة شاشة الفواتير وملخص المبيعات
DB target: 2026.06.01.001
Visible change: Invoices / الفواتير tab with invoice data and sales summary.
```

## Build Version 1.0.1

```powershell
.\tools\build-release.ps1 -Version "1.0.1"
```

Output:

```text
artifacts/releases/v1.0.1/
  manifest.json
  NFOX.DemoApp-1.0.1.zip
  NFOX.Migrations-1.0.1.zip
  checksums.txt
```

## Test Update Locally

Local `file:///` manifest support remains available only for developer testing. Create a local release manifest with `file:///` package URLs:

```powershell
.\tools\create-local-release.ps1 -Version "1.0.1"
```

Copy the printed manifest URL into:

```text
src/NFOX.DemoUpdater/appsettings.json
```

or into the published updater `appsettings.json`, then start the update from the main app:

```powershell
dotnet run --project .\src\NFOX.DemoApp\NFOX.DemoApp.csproj
```

Click `Check for Update` or use the automatic startup check in `NFOX.DemoApp`; do not run the updater manually.

## Update from Main App UI

The intended demo flow is:

```text
Run NFOX.DemoApp.exe -> click Check for Update -> NFOX.DemoUpdater.exe opens
```

Recommended published layout:

```text
install/
  NFOX.DemoApp/
    NFOX.DemoApp.exe
    appsettings.json
  NFOX.DemoUpdater/
    NFOX.DemoUpdater.exe
    appsettings.json
```

`NFOX.DemoApp` resolves `UpdaterPath` relative to the app executable directory. If the configured path is missing, it also searches common sibling layouts, including `..\NFOX.DemoUpdater\NFOX.DemoUpdater.exe`, `.\NFOX.DemoUpdater.exe`, and sibling published/source updater folders.

The `Check for Update` button passes the install directory, current app version, and app config path to the updater. You should not need to run the updater manually from PowerShell.

To test from published folders:

```powershell
.\tools\build-release.ps1 -Version "1.0.0"
.\tools\create-local-release.ps1 -Version "1.0.1"

$install = "F:\NFOX_UPDATE\NFOX.AutoUpdateDemo\install"
New-Item -ItemType Directory -Path "$install\NFOX.DemoApp", "$install\NFOX.DemoUpdater" -Force
Expand-Archive .\artifacts\releases\v1.0.0\NFOX.DemoApp-1.0.0.zip -DestinationPath "$install\NFOX.DemoApp" -Force
Copy-Item .\artifacts\publish\v1.0.0\NFOX.DemoUpdater\* "$install\NFOX.DemoUpdater" -Recurse -Force

& "$install\NFOX.DemoApp\NFOX.DemoApp.exe"
```

Then click `Check for Update`, click `Check` in the updater, then `Download and Update`.

## Publish to GitHub Releases

Build artifacts with GitHub download URLs:

```powershell
.\tools\build-release.ps1 -Version "1.0.1" -Owner "<OWNER>" -Repo "nfox-auto-update-demo"
```

Publish the release:

```powershell
.\tools\publish-github-release.ps1 -Owner "<OWNER>" -Repo "nfox-auto-update-demo" -Version "1.0.1" -ReleaseTitle "NFOX Demo v1.0.1"
```

If the owner is omitted, the script auto-detects it:

```powershell
gh api user --jq .login
```

The updater manifest URL for a public repo is:

```text
https://github.com/<OWNER>/nfox-auto-update-demo/releases/latest/download/manifest.json
```

Detailed developer publishing steps are documented in [docs/developer-update-publishing-guide.md](docs/developer-update-publishing-guide.md).

## GitHub Repository Setup

The proof of concept uses a public repository so the updater can download release assets without credentials:

```powershell
gh repo view <OWNER>/nfox-auto-update-demo
gh repo create nfox-auto-update-demo --public --source . --remote origin --push
```

For a private repository, release assets are not public. Do not embed GitHub credentials in the client app.

## Troubleshooting

- Missing config file: verify `appsettings.json` is copied beside the executable.
- Database connection failed: verify PostgreSQL is running and the connection string is correct.
- Manifest URL invalid: use a valid `file:///` URL or public GitHub release asset URL.
- SHA256 mismatch: rebuild and republish packages so manifest hashes match the assets.
- Migration failed: inspect `logs/migration-yyyy-MM-dd.log`; app files are not replaced when migrations fail.
- App still running: close `NFOX.DemoApp.exe` and retry the updater.
- GitHub CLI not authenticated: run `gh auth login`.

## Oracle 12c Notes

Oracle can be added by implementing `OracleDatabaseProvider` with `Oracle.ManagedDataAccess`, adding Oracle connection string support, and maintaining Oracle-specific migration scripts. DDL behavior differs from PostgreSQL because Oracle often commits DDL implicitly, so scripts need careful testing and idempotent PL/SQL blocks.

Example Oracle syntax:

```sql
ALTER TABLE CUSTOMERS ADD TAX_NO VARCHAR2(50);
```

## Security Notes

- No GitHub tokens are stored in source, configs, manifests, scripts, or logs.
- `logs/`, `downloads/`, `backups/`, and `artifacts/` are ignored by git.
- The PostgreSQL password in configs is a local demo default only.
- Public GitHub Releases are acceptable for this demo, not for production ERP updates.
- Production should use a private update server, protected API, or signed URLs from storage such as Azure Blob Storage or S3-compatible storage.
