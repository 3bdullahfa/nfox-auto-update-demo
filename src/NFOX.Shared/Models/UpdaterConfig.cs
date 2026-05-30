namespace NFOX.Shared.Models;

public sealed class UpdaterConfig
{
    public string AppName { get; set; } = "NFOX ERP Demo";
    public string CurrentAppVersion { get; set; } = "1.0.0";
    public string UpdateSource { get; set; } = "GitHub";
    public string GitHubOwner { get; set; } = "3bdullahfa";
    public string GitHubRepo { get; set; } = "nfox-auto-update-channel";
    public bool GitHubUseLatestRelease { get; set; } = true;
    public string ManifestUrl { get; set; } = "https://github.com/3bdullahfa/nfox-auto-update-channel/releases/latest/download/manifest.json";
    public string InstallDirectory { get; set; } = "../NFOX.DemoApp";
    public string BackupDirectory { get; set; } = "../../backups";
    public string DownloadDirectory { get; set; } = "../../downloads";
    public string DatabaseProvider { get; set; } = "PostgreSQL";
    public string ConnectionString { get; set; } = "";
}
