namespace NFOX.Shared.Models;

public sealed class AppConfig
{
    public string AppName { get; set; } = "NFOX ERP Demo";
    public string AppVersion { get; set; } = "1.0.0";
    public string UpdateName { get; set; } = "Initial Release";
    public string DatabaseProvider { get; set; } = "PostgreSQL";
    public string ConnectionString { get; set; } = "";
    public string UpdaterPath { get; set; } = "../NFOX.DemoUpdater/NFOX.DemoUpdater.exe";
}
