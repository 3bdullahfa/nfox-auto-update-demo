using System.Data;

namespace NFOX.Shared.Database;

public interface IDatabaseProvider
{
    string ProviderName { get; }
    IDbConnection CreateConnection(string connectionString);
}
