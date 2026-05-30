namespace NFOX.Shared.Models;

public sealed class UpdaterConfig
{
    public string AppName { get; set; } = "NFOX ERP Demo";
    public string CurrentAppVersion { get; set; } = "1.0.0";
    public string ManifestUrl { get; set; } = "PUT_GITHUB_RELEASE_MANIFEST_URL_HERE";
    public string InstallDirectory { get; set; } = "../NFOX.DemoApp";
    public string BackupDirectory { get; set; } = "../../backups";
    public string DownloadDirectory { get; set; } = "../../downloads";
    public string DatabaseProvider { get; set; } = "PostgreSQL";
    public string ConnectionString { get; set; } = "";
}
