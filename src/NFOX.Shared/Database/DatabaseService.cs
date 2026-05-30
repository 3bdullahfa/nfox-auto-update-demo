using System.Data;
using NFOX.Shared.Services;

namespace NFOX.Shared.Database;

public sealed class DatabaseService
{
    private readonly IDatabaseProvider _provider;
    private readonly string _connectionString;
    private readonly LogService _logger;

    public DatabaseService(IDatabaseProvider provider, string connectionString, LogService logger)
    {
        _provider = provider;
        _connectionString = connectionString;
        _logger = logger;
    }

    public string ProviderName => _provider.ProviderName;

    public void TestConnection()
    {
        using var connection = _provider.CreateConnection(_connectionString);
        connection.Open();
    }

    public void EnsureMigrationTables()
    {
        using var connection = _provider.CreateConnection(_connectionString);
        connection.Open();
        ExecuteNonQuery(connection, SchemaVersionTableSql);
        ExecuteNonQuery(connection, UpdateLockTableSql);
        _logger.Info("Database", "Ensured migration and update lock tables.");
    }

    public string GetCurrentSchemaVersion()
    {
        EnsureMigrationTables();
        using var connection = _provider.CreateConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT version_no
            FROM nfox_schema_version
            WHERE status = 'SUCCESS'
            ORDER BY applied_at DESC, id DESC
            LIMIT 1;
            """;

        var value = command.ExecuteScalar();
        return value?.ToString() ?? "Not installed";
    }

    public DataTable LoadCustomers()
    {
        using var connection = _provider.CreateConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM customers ORDER BY id;";
        using var reader = command.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    private static void ExecuteNonQuery(IDbConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    internal const string SchemaVersionTableSql = """
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
        """;

    internal const string UpdateLockTableSql = """
        CREATE TABLE IF NOT EXISTS nfox_update_lock (
            lock_id INT PRIMARY KEY,
            machine_name VARCHAR(200),
            locked_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
        );
        """;
}
