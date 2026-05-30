using System.Data;
using NFOX.Shared.Models;
using NFOX.Shared.Services;

namespace NFOX.Shared.Database;

public sealed class MigrationRunner
{
    private readonly IDatabaseProvider _provider;
    private readonly string _connectionString;
    private readonly LogService _logger;

    public MigrationRunner(IDatabaseProvider provider, string connectionString, LogService logger)
    {
        _provider = provider;
        _connectionString = connectionString;
        _logger = logger;
    }

    public MigrationRunResult RunMigrations(string migrationsDirectory, string appVersion)
    {
        var result = new MigrationRunResult();
        if (!Directory.Exists(migrationsDirectory))
        {
            result.Success = false;
            result.Message = $"Migrations directory not found: {migrationsDirectory}";
            return result;
        }

        var scripts = Directory.GetFiles(migrationsDirectory, "*.sql", SearchOption.TopDirectoryOnly)
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        using var connection = _provider.CreateConnection(_connectionString);
        connection.Open();
        EnsureTables(connection);

        var lockAcquired = false;
        try
        {
            AcquireLock(connection);
            lockAcquired = true;
            _logger.Info("MigrationLock", "Database update lock acquired.");

            foreach (var scriptPath in scripts)
            {
                var scriptName = Path.GetFileName(scriptPath);
                var versionNo = ExtractVersionNo(scriptName);
                var checksum = FileHashService.ComputeSha256(scriptPath);

                if (HasSuccessfulMigration(connection, versionNo, scriptName))
                {
                    _logger.Info("Migration", $"Skipped already applied migration {scriptName}.");
                    result.Migrations.Add(new MigrationExecutionResult
                    {
                        VersionNo = versionNo,
                        ScriptName = scriptName,
                        Status = "SKIPPED",
                        Message = "Already applied successfully."
                    });
                    continue;
                }

                var sql = File.ReadAllText(scriptPath);
                using var transaction = connection.BeginTransaction();
                try
                {
                    ExecuteNonQuery(connection, transaction, sql);
                    InsertHistory(connection, transaction, versionNo, scriptName, checksum, "SUCCESS", null, appVersion);
                    transaction.Commit();
                    _logger.Info("Migration", $"Applied migration {scriptName}.");
                    result.Migrations.Add(new MigrationExecutionResult
                    {
                        VersionNo = versionNo,
                        ScriptName = scriptName,
                        Status = "SUCCESS",
                        Message = "Migration applied."
                    });
                }
                catch (Exception ex)
                {
                    TryRollback(transaction);
                    InsertFailureHistory(connection, versionNo, scriptName, checksum, ex, appVersion);
                    _logger.Error("Migration", $"Migration failed: {scriptName}", ex);
                    result.Success = false;
                    result.Message = $"Migration failed: {scriptName}. {ex.Message}";
                    result.CurrentDbVersion = GetCurrentSchemaVersion(connection);
                    result.Migrations.Add(new MigrationExecutionResult
                    {
                        VersionNo = versionNo,
                        ScriptName = scriptName,
                        Status = "FAILED",
                        Message = ex.Message
                    });
                    return result;
                }
            }

            result.Success = true;
            result.CurrentDbVersion = GetCurrentSchemaVersion(connection);
            result.Message = "Migrations completed successfully.";
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error("Migration", "Migration run failed.", ex);
            result.Success = false;
            result.Message = ex.Message;
            result.CurrentDbVersion = GetCurrentSchemaVersionSafe(connection);
            return result;
        }
        finally
        {
            if (lockAcquired)
            {
                ReleaseLock(connection);
                _logger.Info("MigrationLock", "Database update lock released.");
            }
        }
    }

    private static string ExtractVersionNo(string scriptName)
    {
        var marker = scriptName.IndexOf("__", StringComparison.Ordinal);
        if (marker <= 0)
        {
            throw new FormatException($"Invalid migration file name: {scriptName}. Expected YYYY.MM.DD.NNN__description.sql.");
        }

        return scriptName[..marker];
    }

    private static void EnsureTables(IDbConnection connection)
    {
        ExecuteNonQuery(connection, null, DatabaseService.SchemaVersionTableSql);
        ExecuteNonQuery(connection, null, DatabaseService.UpdateLockTableSql);
    }

    private static bool HasSuccessfulMigration(IDbConnection connection, string versionNo, string scriptName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM nfox_schema_version
            WHERE version_no = @version_no
              AND script_name = @script_name
              AND status = 'SUCCESS';
            """;
        AddParameter(command, "@version_no", versionNo);
        AddParameter(command, "@script_name", scriptName);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static void AcquireLock(IDbConnection connection)
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT machine_name, locked_at FROM nfox_update_lock WHERE lock_id = 1;";
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var machine = reader.IsDBNull(0) ? "" : reader.GetString(0);
                var lockedAt = reader.GetDateTime(1);
                if (!machine.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase) &&
                    lockedAt > DateTime.Now.AddMinutes(-30))
                {
                    throw new InvalidOperationException($"Database update lock is held by {machine} since {lockedAt:yyyy-MM-dd HH:mm:ss}.");
                }
            }
        }

        using var upsert = connection.CreateCommand();
        upsert.CommandText = """
            INSERT INTO nfox_update_lock (lock_id, machine_name, locked_at)
            VALUES (1, @machine_name, CURRENT_TIMESTAMP)
            ON CONFLICT (lock_id)
            DO UPDATE SET machine_name = EXCLUDED.machine_name, locked_at = CURRENT_TIMESTAMP;
            """;
        AddParameter(upsert, "@machine_name", Environment.MachineName);
        upsert.ExecuteNonQuery();
    }

    private static void ReleaseLock(IDbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM nfox_update_lock WHERE lock_id = 1 AND machine_name = @machine_name;";
        AddParameter(command, "@machine_name", Environment.MachineName);
        command.ExecuteNonQuery();
    }

    private static void InsertHistory(IDbConnection connection, IDbTransaction? transaction, string versionNo, string scriptName, string checksum, string status, string? errorMessage, string appVersion)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO nfox_schema_version
                (version_no, script_name, checksum, status, error_message, machine_name, app_version)
            VALUES
                (@version_no, @script_name, @checksum, @status, @error_message, @machine_name, @app_version);
            """;
        AddParameter(command, "@version_no", versionNo);
        AddParameter(command, "@script_name", scriptName);
        AddParameter(command, "@checksum", checksum);
        AddParameter(command, "@status", status);
        AddParameter(command, "@error_message", (object?)errorMessage ?? DBNull.Value);
        AddParameter(command, "@machine_name", Environment.MachineName);
        AddParameter(command, "@app_version", appVersion);
        command.ExecuteNonQuery();
    }

    private static void InsertFailureHistory(IDbConnection connection, string versionNo, string scriptName, string checksum, Exception exception, string appVersion)
    {
        InsertHistory(connection, null, versionNo, scriptName, checksum, "FAILED", exception.ToString(), appVersion);
    }

    private static string GetCurrentSchemaVersion(IDbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT version_no
            FROM nfox_schema_version
            WHERE status = 'SUCCESS'
            ORDER BY applied_at DESC, id DESC
            LIMIT 1;
            """;
        return command.ExecuteScalar()?.ToString() ?? "Not installed";
    }

    private static string GetCurrentSchemaVersionSafe(IDbConnection connection)
    {
        try
        {
            return GetCurrentSchemaVersion(connection);
        }
        catch
        {
            return "Unknown";
        }
    }

    private static void ExecuteNonQuery(IDbConnection connection, IDbTransaction? transaction, string sql)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static void TryRollback(IDbTransaction transaction)
    {
        try
        {
            transaction.Rollback();
        }
        catch
        {
        }
    }
}
