# Update Flow

```text
DemoApp
  -> Check for Update
  -> DemoUpdater
  -> manifest.json
  -> download app ZIP and migration ZIP
  -> verify SHA256
  -> backup install directory
  -> acquire database update lock
  -> apply pending migrations
  -> write migration history
  -> release database update lock
  -> replace app files
  -> update local appsettings.json
  -> launch updated DemoApp
```

Safety rule: application files are not replaced until database migrations complete successfully.

If a migration fails, the updater logs the error, records a failed migration row, keeps the backup, and leaves the existing app files in place.
