# Oracle Future Support

The architecture supports additional database providers through:

```csharp
public interface IDatabaseProvider
{
    string ProviderName { get; }
    IDbConnection CreateConnection(string connectionString);
}
```

To support Oracle 12c:

- Add `Oracle.ManagedDataAccess`.
- Implement `OracleDatabaseProvider`.
- Add Oracle connection strings such as `User Id=nfox;Password=secret;Data Source=localhost:1521/ORCLPDB1`.
- Maintain Oracle-specific migration folders or release packages.
- Test every DDL script against Oracle before release.

Oracle DDL often commits implicitly, so transaction behavior differs from PostgreSQL. Use safe and idempotent PL/SQL blocks for conditional schema changes.

Oracle example:

```sql
ALTER TABLE CUSTOMERS ADD TAX_NO VARCHAR2(50);
```

PostgreSQL equivalent:

```sql
ALTER TABLE customers ADD COLUMN IF NOT EXISTS tax_no VARCHAR(50);
```

Recommended Oracle pattern:

```sql
DECLARE
    column_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO column_count
    FROM user_tab_columns
    WHERE table_name = 'CUSTOMERS'
      AND column_name = 'TAX_NO';

    IF column_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE CUSTOMERS ADD TAX_NO VARCHAR2(50)';
    END IF;
END;
/
```
