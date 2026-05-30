using System.Data;

namespace NFOX.Shared.Database;

public sealed class OracleDatabaseProvider : IDatabaseProvider
{
    public string ProviderName => "Oracle";

    public IDbConnection CreateConnection(string connectionString)
    {
        throw new NotImplementedException("Oracle support is a placeholder in this proof of concept. Add Oracle.ManagedDataAccess and Oracle-specific migration scripts before enabling it.");
    }
}
