namespace NFOX.Shared.Models;

public sealed class MigrationInfo
{
    public string VersionNo { get; set; } = "";
    public string ScriptName { get; set; } = "";
    public string Checksum { get; set; } = "";
    public DateTime AppliedAt { get; set; }
    public string Status { get; set; } = "";
    public string? ErrorMessage { get; set; }
}

public sealed class MigrationExecutionResult
{
    public string VersionNo { get; set; } = "";
    public string ScriptName { get; set; } = "";
    public string Status { get; set; } = "";
    public string Message { get; set; } = "";
}

public sealed class MigrationRunResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string CurrentDbVersion { get; set; } = "";
    public List<MigrationExecutionResult> Migrations { get; } = new();
}
