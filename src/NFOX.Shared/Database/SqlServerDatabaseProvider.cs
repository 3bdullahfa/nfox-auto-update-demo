using System.Data;

namespace NFOX.Shared.Database;

public sealed class SqlServerDatabaseProvider : IDatabaseProvider
{
    public string ProviderName => "SQL Server";

    public IDbConnection CreateConnection(string connectionString)
    {
        throw new NotImplementedException("SQL Server support is a placeholder in this proof of concept. Add Microsoft.Data.SqlClient and SQL Server migration scripts before enabling it.");
    }
}
