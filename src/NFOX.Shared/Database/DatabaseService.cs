using System.Data;
using NFOX.Shared.Models;
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

    public DataTable LoadInvoices()
    {
        using var connection = _provider.CreateConnection(_connectionString);
        connection.Open();
        if (!TableExists(connection, "invoices"))
        {
            _logger.Warning("Invoices", "Invoices table does not exist yet.");
            return CreateEmptyInvoicesTable();
        }

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                i.id,
                i.invoice_no,
                i.invoice_date,
                i.customer_id,
                c.customer_name,
                i.total_amount,
                i.notes
            FROM invoices i
            LEFT JOIN customers c ON c.id = i.customer_id
            ORDER BY i.invoice_date DESC, i.id DESC;
            """;
        using var reader = command.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public InvoiceSummary GetInvoiceSummary()
    {
        using var connection = _provider.CreateConnection(_connectionString);
        connection.Open();
        if (!TableExists(connection, "invoices"))
        {
            return new InvoiceSummary();
        }

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                COUNT(*) AS invoice_count,
                COALESCE(SUM(total_amount), 0) AS total_amount,
                MAX(invoice_date) AS latest_invoice_date
            FROM invoices;
            """;
        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return new InvoiceSummary();
        }

        return new InvoiceSummary
        {
            InvoiceCount = Convert.ToInt32(reader["invoice_count"]),
            TotalAmount = Convert.ToDecimal(reader["total_amount"]),
            LatestInvoiceDate = reader["latest_invoice_date"] == DBNull.Value
                ? null
                : Convert.ToDateTime(reader["latest_invoice_date"])
        };
    }

    private static void ExecuteNonQuery(IDbConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static bool TableExists(IDbConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_name = @table_name;
            """;
        AddParameter(command, "@table_name", tableName);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static DataTable CreateEmptyInvoicesTable()
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("invoice_no", typeof(string));
        table.Columns.Add("invoice_date", typeof(DateTime));
        table.Columns.Add("customer_id", typeof(int));
        table.Columns.Add("customer_name", typeof(string));
        table.Columns.Add("total_amount", typeof(decimal));
        table.Columns.Add("notes", typeof(string));
        return table;
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
