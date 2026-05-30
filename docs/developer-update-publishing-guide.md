# Developer Update Publishing Guide

This guide explains how to build a new NFOX Auto Update Demo release from the source repository and publish compiled assets to the releases-only update channel.

## Production-like Update Distribution

The project uses two repositories:

```text
Source repository: 3bdullahfa/nfox-auto-update-demo
Update channel:    3bdullahfa/nfox-auto-update-channel
```

The source repository is the developer workspace. It can contain source code, build scripts, docs, and migrations. The update channel is public for this proof of concept so clients can download assets without credentials, but it must not receive source code.

The update channel should contain only:

- A minimal README.
- GitHub Releases.
- Release assets: `manifest.json`, `NFOX.UpdatePackage-X.Y.Z.zip`, and `checksums.txt`.

Do not publish `src/`, `.git/`, `database/`, `tools/`, `docs/`, `*.cs`, `*.csproj`, `*.sln`, or PowerShell source build scripts to the update channel.

Public GitHub Releases are acceptable for the demo. For production ERP software, use a private update server, protected API, Azure Blob Storage or S3-compatible storage with signed URLs, a CDN with signed URLs, and signed packages. Compiled binaries can still be reverse-engineered, so this separation protects source distribution but is not complete intellectual-property protection.

## 1. Update App Version

Update the app version and update name in:

```text
src/NFOX.DemoApp/appsettings.json
```

Example:

```json
{
  "appVersion": "1.0.3",
  "updateName": "New update name"
}
```

The build script also writes the selected `-Version` into the published app package.

## 2. Add Database Migration

Add migration SQL files under:

```text
releases/v1.0.3/migrations/
```

Migration file names must follow:

```text
YYYY.MM.DD.NNN__description.sql
```

Example:

```text
2026.06.01.002__add_invoice_notes.sql
```

Migrations should be idempotent where possible.

PostgreSQL example:

```sql
ALTER TABLE invoices
ADD COLUMN IF NOT EXISTS notes TEXT;
```

Do not edit migrations that have already been applied on a client database. Publish a new forward-fix migration instead.

## 3. Build Release Artifacts

```powershell
cd F:\NFOX_UPDATE\NFOX.AutoUpdateDemo

dotnet build -c Release

.\tools\build-release.ps1 `
  -Version "1.0.3" `
  -Owner "3bdullahfa" `
  -Repo "nfox-auto-update-channel"
```

Expected output:

```text
artifacts/releases/v1.0.3/
  manifest.json
  NFOX.UpdatePackage-1.0.3.zip
  checksums.txt
```

The ZIP should contain compiled binaries and migration output only:

```text
NFOX.UpdatePackage-1.0.3/
  app/
  updater/
  migrations/
  manifest.json
```

## 4. Review manifest.json

Before publishing, review:

```text
artifacts/releases/v1.0.3/manifest.json
```

The manifest must contain:

- `latestAppVersion`
- `updateName`
- `targetDbVersion`
- `releaseNotes`
- `packages.updatePackage.fileName`
- `packages.updatePackage.downloadUrl`
- `packages.updatePackage.sha256`

The package URL should point to the update channel repository:

```text
https://github.com/3bdullahfa/nfox-auto-update-channel/releases/download/v1.0.3/NFOX.UpdatePackage-1.0.3.zip
```

## 5. Publish Update Channel Release

```powershell
.\tools\publish-update-channel-release.ps1 `
  -Owner "3bdullahfa" `
  -Repo "nfox-auto-update-channel" `
  -Version "1.0.3" `
  -ReleaseTitle "NFOX Demo v1.0.3"
```

The script uses the existing authenticated GitHub CLI session. It checks `gh auth status`, creates `3bdullahfa/nfox-auto-update-channel` if missing, and uploads only release assets from `artifacts/releases/vX.Y.Z/`.

The script does not use `--source .`, does not add a remote, and does not push the source tree.

## 6. Verify Release Assets

```powershell
gh release view v1.0.3 --repo 3bdullahfa/nfox-auto-update-channel
```

Expected assets:

```text
manifest.json
NFOX.UpdatePackage-1.0.3.zip
checksums.txt
```

The stable client manifest URL is:

```text
https://github.com/3bdullahfa/nfox-auto-update-channel/releases/latest/download/manifest.json
```

Verify that the update channel repository has no source tree:

```powershell
gh repo view 3bdullahfa/nfox-auto-update-channel
gh api repos/3bdullahfa/nfox-auto-update-channel/contents --jq ".[].name"
```

Only a minimal `README.md` should be present in repository contents. The compiled artifacts should be release assets, not committed files.

## 7. Test Client Update

The client should only run:

```text
NFOX.DemoApp.exe
```

The main app checks the update channel automatically. If an update exists, click the in-app update button. The user should not manually edit manifest URLs or run `NFOX.DemoUpdater.exe` from PowerShell.

The updater should:

1. Discover the latest release in `3bdullahfa/nfox-auto-update-channel`.
2. Download `manifest.json`.
3. Download `NFOX.UpdatePackage-X.Y.Z.zip`.
4. Verify SHA256 from the manifest.
5. Extract `app/`, `updater/`, and `migrations/`.
6. Apply database migrations.
7. Replace application files.
8. Relaunch `NFOX.DemoApp`.

## 8. Rollback and Safety Notes

- Do not publish destructive migrations without a backup strategy.
- Do not manually edit applied migrations.
- If a migration is wrong, publish a new forward-fix migration.
- Keep backups.
- Test locally before publishing to GitHub.
- Do not embed GitHub credentials in the client app.
- Do not use public GitHub Releases for production ERP binaries.

## Example: Publishing version 1.0.2 with app and database changes

Version `1.0.2` adds the invoices UI and the database objects needed by that screen.

1. Ensure the migration file exists:

```text
releases/v1.0.2/migrations/2026.06.01.001__add_invoices_module.sql
```

2. Build release artifacts:

```powershell
.\tools\build-release.ps1 -Version "1.0.2" -Owner "3bdullahfa" -Repo "nfox-auto-update-channel"
```

3. Publish the update-channel release:

```powershell
.\tools\publish-update-channel-release.ps1 -Owner "3bdullahfa" -Repo "nfox-auto-update-channel" -Version "1.0.2" -ReleaseTitle "NFOX Demo v1.0.2"
```

4. Verify GitHub assets:

```powershell
gh release view v1.0.2 --repo 3bdullahfa/nfox-auto-update-channel
```

Expected assets:

```text
manifest.json
NFOX.UpdatePackage-1.0.2.zip
checksums.txt
```

5. Test client update from the UI:

```text
Run NFOX.DemoApp.exe -> wait for the update panel -> click the in-app update button
```
