# Developer Update Publishing Guide

This guide explains how to publish a new NFOX Auto Update Demo release through GitHub Releases.

## 1. Update App Version

Update the app version and update name in:

```text
src/NFOX.DemoApp/appsettings.json
```

Example:

```json
{
  "appVersion": "1.0.2",
  "updateName": "اسم التحديث الجديد"
}
```

The build script also writes the selected `-Version` into the published app package.

## 2. Add Database Migration

Add migration SQL files under:

```text
releases/v1.0.2/migrations/
```

Migration file names must follow:

```text
YYYY.MM.DD.NNN__description.sql
```

Example:

```text
2026.06.01.001__add_invoice_notes.sql
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
.\tools\build-release.ps1 -Version "1.0.2" -Owner "3bdullahfa" -Repo "nfox-auto-update-demo"
```

Expected output:

```text
artifacts/releases/v1.0.2/
  manifest.json
  NFOX.DemoApp-1.0.2.zip
  NFOX.Migrations-1.0.2.zip
  checksums.txt
```

## 4. Review manifest.json

Before publishing, review:

```text
artifacts/releases/v1.0.2/manifest.json
```

The manifest must contain:

- `latestAppVersion`
- `updateName`
- `targetDbVersion`
- `releaseNotes`
- `downloadUrl`
- `sha256`

The package URLs should point to GitHub release assets for the target version.

## 5. Publish GitHub Release

```powershell
.\tools\publish-github-release.ps1 `
  -Owner "3bdullahfa" `
  -Repo "nfox-auto-update-demo" `
  -Version "1.0.2" `
  -ReleaseTitle "NFOX Demo v1.0.2"
```

The script uses the existing authenticated GitHub CLI session. It does not require client-side GitHub credentials and does not write tokens to app config.

## 6. Verify Release Assets

```powershell
gh release view v1.0.2 --repo 3bdullahfa/nfox-auto-update-demo
```

Expected assets:

```text
manifest.json
NFOX.DemoApp-1.0.2.zip
NFOX.Migrations-1.0.2.zip
checksums.txt
```

The stable client manifest URL is:

```text
https://github.com/3bdullahfa/nfox-auto-update-demo/releases/latest/download/manifest.json
```

## 7. Test Client Update

The client should only run:

```text
NFOX.DemoApp.exe
```

The main app checks GitHub automatically. If an update exists, click:

```text
تحديث الآن
```

The user should not manually edit manifest URLs or run `NFOX.DemoUpdater.exe` from PowerShell.

## 8. Rollback and Safety Notes

- Do not publish destructive migrations without a backup strategy.
- Do not manually edit applied migrations.
- If a migration is wrong, publish a new forward-fix migration.
- Keep backups.
- Test locally before publishing to GitHub.
- Do not use public GitHub Releases for production ERP binaries.

## Example: Publishing version 1.0.2 with app and database changes

Version `1.0.2` adds the invoices UI and the database objects needed by that screen.

1. Change app version and update name:

```json
{
  "appVersion": "1.0.2",
  "updateName": "إضافة شاشة الفواتير وملخص المبيعات"
}
```

2. Add migration file:

```text
releases/v1.0.2/migrations/2026.06.01.001__add_invoices_module.sql
```

The migration creates `invoices`, adds `customers.customer_category`, and seeds demo invoices.

3. Build release artifacts:

```powershell
.\tools\build-release.ps1 -Version "1.0.2" -Owner "3bdullahfa" -Repo "nfox-auto-update-demo"
```

4. Publish GitHub Release:

```powershell
.\tools\publish-github-release.ps1 -Owner "3bdullahfa" -Repo "nfox-auto-update-demo" -Version "1.0.2" -ReleaseTitle "NFOX Demo v1.0.2"
```

5. Verify GitHub assets:

```powershell
gh release view v1.0.2 --repo 3bdullahfa/nfox-auto-update-demo
```

Expected assets:

```text
manifest.json
NFOX.DemoApp-1.0.2.zip
NFOX.Migrations-1.0.2.zip
checksums.txt
```

6. Test client update from UI:

```text
Run NFOX.DemoApp.exe -> wait for the GitHub update panel -> click تحديث الآن
```

7. Safety notes:

- Test locally before publishing.
- Do not modify already-applied migrations.
- Publish forward-fix migrations if something goes wrong.
- Keep database backups before production updates.
