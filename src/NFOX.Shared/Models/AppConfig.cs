namespace NFOX.Shared.Models;

public sealed class AppConfig
{
    public string AppName { get; set; } = "NFOX ERP Demo";
    public string AppVersion { get; set; } = "1.0.0";
    public string UpdateName { get; set; } = "Initial Release";
    public string DatabaseProvider { get; set; } = "PostgreSQL";
    public string ConnectionString { get; set; } = "";
    public string UpdaterPath { get; set; } = "../NFOX.DemoUpdater/NFOX.DemoUpdater.exe";
    public string UpdateSource { get; set; } = "GitHub";
    public string GitHubOwner { get; set; } = "3bdullahfa";
    public string GitHubRepo { get; set; } = "nfox-auto-update-channel";
    public bool GitHubUseLatestRelease { get; set; } = true;
    public string ManifestUrl { get; set; } = "https://github.com/3bdullahfa/nfox-auto-update-channel/releases/latest/download/manifest.json";
    public bool AutoCheckForUpdatesOnStartup { get; set; } = true;
    public bool ShowNoUpdateMessage { get; set; }
}
