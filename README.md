# NFOX Auto Update Demo

Proof of concept for a Windows Forms accounting/ERP-style application that updates application files and PostgreSQL schema over a versioned update channel.

## Architecture

```text
NFOX.DemoApp
  -> checks GitHub Releases automatically on startup
  -> shows an in-app update notification when a newer version exists
  -> launches NFOX.DemoUpdater when the user clicks Check for Update
      -> discovers the latest GitHub release
      -> downloads manifest.json from the update channel
      -> downloads NFOX.UpdatePackage-X.Y.Z.zip
      -> verifies SHA256
      -> backs up the current app folder
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

## Run Demo App

```powershell
dotnet run --project .\src\NFOX.DemoApp\NFOX.DemoApp.csproj
```

The app checks GitHub automatically and shows an update notification when a newer release is available.

## Production-like Update Distribution

The source repository and the update distribution channel are separate:

```text
Source repository: 3bdullahfa/nfox-auto-update-demo
Update channel:    3bdullahfa/nfox-auto-update-channel
```

The source repository can remain private or internal and contains code, tools, docs, migrations, and developer material. The public update channel must not receive source code commits. It should contain only a minimal README plus GitHub Releases assets.

Expected release assets in the update channel:

```text
manifest.json
NFOX.UpdatePackage-X.Y.Z.zip
checksums.txt
```

`NFOX.UpdatePackage-X.Y.Z.zip` contains compiled output only:

```text
NFOX.UpdatePackage-X.Y.Z/
  app/
    NFOX.DemoApp.exe
    appsettings.json
    dependencies...
  updater/
    NFOX.DemoUpdater.exe
    appsettings.json
    dependencies...
  migrations/
    YYYY.MM.DD.NNN__description.sql
  manifest.json
```

Do not upload `src/`, `.git/`, `database/`, `tools/`, `docs/`, `*.cs`, `*.csproj`, `*.sln`, or build scripts to the update channel. Public GitHub Releases are useful for this demo, but proprietary ERP binaries can still be reverse-engineered. Production systems should use a private update server, protected API, CDN or object storage with signed URLs, and package signing.

## GitHub Automatic Update Flow

GitHub Releases in the distribution repository are the default update channel:

```text
Owner: 3bdullahfa
Repo: nfox-auto-update-channel
Latest manifest: https://github.com/3bdullahfa/nfox-auto-update-channel/releases/latest/download/manifest.json
```

`NFOX.DemoApp` reads its local version from `appsettings.json`, checks the update channel on startup, downloads the latest `manifest.json`, and compares versions using semantic version comparison.

If an update exists, the main app shows the update panel with the current version, new version, update name, release notes, target database version, and required-update flag. The updater downloads the update package from GitHub Releases, verifies SHA256, applies database migrations, replaces files only after migrations succeed, and relaunches the updated app.

Latest demo update:

```text
Version: 1.0.2
DB target: 2026.06.01.001
Visible change: Invoices tab with invoice data and sales summary.
```

## Build Release Artifacts

```powershell
.\tools\build-release.ps1 -Version "1.0.2" -Owner "3bdullahfa" -Repo "nfox-auto-update-channel"
```

Output:

```text
artifacts/releases/v1.0.2/
  manifest.json
  NFOX.UpdatePackage-1.0.2.zip
  checksums.txt
```

## Test Update Locally

Local `file:///` manifest support remains available only for developer testing. Create a local release manifest with a `file:///` update package URL:

```powershell
.\tools\create-local-release.ps1 -Version "1.0.2"
```

Copy the printed manifest URL into the published updater `appsettings.json`, then start the update from the main app. Do not run the updater manually from PowerShell.

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

The `Check for Update` button passes the install directory, current app version, app config path, GitHub owner, GitHub repo, and manifest URL to the updater. You should not need to run the updater manually from PowerShell.

To create an install folder from a built package:

```powershell
.\tools\build-release.ps1 -Version "1.0.0"

$install = "F:\NFOX_UPDATE\NFOX.AutoUpdateDemo\install"
$stage = "F:\NFOX_UPDATE\NFOX.AutoUpdateDemo\artifacts\install-source\v1.0.0"
Remove-Item $stage, $install -Recurse -Force -ErrorAction SilentlyContinue
Expand-Archive .\artifacts\releases\v1.0.0\NFOX.UpdatePackage-1.0.0.zip -DestinationPath $stage -Force
New-Item -ItemType Directory -Path "$install\NFOX.DemoApp", "$install\NFOX.DemoUpdater" -Force
Copy-Item "$stage\NFOX.UpdatePackage-1.0.0\app\*" "$install\NFOX.DemoApp" -Recurse -Force
Copy-Item "$stage\NFOX.UpdatePackage-1.0.0\updater\*" "$install\NFOX.DemoUpdater" -Recurse -Force

& "$install\NFOX.DemoApp\NFOX.DemoApp.exe"
```

Then click `Check for Update`, click `Check` in the updater if needed, then `Download and Update`.

## Publish to Update Channel

Build artifacts with update-channel download URLs:

```powershell
.\tools\build-release.ps1 -Version "1.0.2" -Owner "3bdullahfa" -Repo "nfox-auto-update-channel"
```

Publish the release assets only:

```powershell
.\tools\publish-update-channel-release.ps1 -Owner "3bdullahfa" -Repo "nfox-auto-update-channel" -Version "1.0.2" -ReleaseTitle "NFOX Demo v1.0.2"
```

The script uses the existing authenticated GitHub CLI session, creates the update channel repository if missing, and never uses `--source .` or pushes the source tree.

The updater manifest URL for the public update channel is:

```text
https://github.com/3bdullahfa/nfox-auto-update-channel/releases/latest/download/manifest.json
```

Detailed developer publishing steps are documented in [docs/developer-update-publishing-guide.md](docs/developer-update-publishing-guide.md).

## GitHub Repository Setup

The proof of concept uses a public releases-only repository so the updater can download release assets without credentials:

```powershell
gh repo view 3bdullahfa/nfox-auto-update-channel
gh release view v1.0.2 --repo 3bdullahfa/nfox-auto-update-channel
```

For production, do not embed GitHub credentials in the client app.

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
- Production should use a private update server, protected API, signed URLs from storage such as Azure Blob Storage or S3-compatible storage, and package signing.
- Binaries can be reverse-engineered. A releases-only channel protects source distribution, not all intellectual property.
