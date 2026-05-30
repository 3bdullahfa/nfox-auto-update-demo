# Database Migration Rules

Migration files use this naming format:

```text
YYYY.MM.DD.NNN__description.sql
```

Example:

```text
2026.05.30.002__add_tax_no_to_customers.sql
```

Rules:

- Migrations are sorted by file name.
- The version number is the part before `__`.
- SHA256 is computed for every SQL file.
- A migration is skipped when the same version and script name already has `SUCCESS` in `nfox_schema_version`.
- Pending migrations run inside a database transaction where possible.
- Success and failure rows are written to `nfox_schema_version`.
- The updater stops on the first failed migration.
- Scripts should be idempotent where possible.

PostgreSQL example:

```sql
ALTER TABLE customers ADD COLUMN IF NOT EXISTS tax_no VARCHAR(50);
```

SQL Server example:

```sql
IF COL_LENGTH('customers', 'tax_no') IS NULL
BEGIN
    ALTER TABLE customers ADD tax_no VARCHAR(50);
END
```

Rollback is not automatic in this proof of concept. For production, pair schema changes with tested rollback or forward-fix scripts and keep backups before release.
