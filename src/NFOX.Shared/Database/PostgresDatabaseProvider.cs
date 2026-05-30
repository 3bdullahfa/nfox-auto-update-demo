using System.Data;
using Npgsql;

namespace NFOX.Shared.Database;

public sealed class PostgresDatabaseProvider : IDatabaseProvider
{
    public string ProviderName => "PostgreSQL";

    public IDbConnection CreateConnection(string connectionString) => new NpgsqlConnection(connectionString);
}
