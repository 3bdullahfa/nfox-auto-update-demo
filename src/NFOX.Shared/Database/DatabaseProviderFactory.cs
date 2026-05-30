namespace NFOX.Shared.Database;

public static class DatabaseProviderFactory
{
    public static IDatabaseProvider Create(string providerName)
    {
        var normalized = providerName.Trim().ToLowerInvariant();
        return normalized switch
        {
            "postgres" or "postgresql" => new PostgresDatabaseProvider(),
            "sqlserver" or "sql server" or "mssql" => new SqlServerDatabaseProvider(),
            "oracle" => new OracleDatabaseProvider(),
            _ => throw new NotSupportedException($"Unsupported database provider: {providerName}")
        };
    }
}
