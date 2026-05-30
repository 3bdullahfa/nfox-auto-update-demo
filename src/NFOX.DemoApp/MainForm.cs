using NFOX.Shared.Database;
using NFOX.Shared.Models;
using NFOX.Shared.Services;

namespace NFOX.DemoApp;

public partial class MainForm : Form
{
    private readonly string _configPath;
    private readonly LogService _logger;
    private AppConfig? _config;

    public MainForm()
    {
        InitializeComponent();
        _configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        _logger = new LogService("app");
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        RefreshApplicationData();
    }

    private void btnRefresh_Click(object? sender, EventArgs e)
    {
        RefreshApplicationData();
    }

    private void btnCheckForUpdate_Click(object? sender, EventArgs e)
    {
        try
        {
            EnsureConfigLoaded();
            var updaterResolution = ResolveUpdaterPath(_config!.UpdaterPath);
            if (updaterResolution.Path is null)
            {
                var searchedPaths = string.Join(Environment.NewLine, updaterResolution.SearchedPaths.Select(path => $"  - {path}"));
                var message = $"NFOX.DemoUpdater.exe was not found.{Environment.NewLine}{Environment.NewLine}" +
                    $"Expected layout:{Environment.NewLine}" +
                    $"install\\NFOX.DemoApp\\NFOX.DemoApp.exe{Environment.NewLine}" +
                    $"install\\NFOX.DemoUpdater\\NFOX.DemoUpdater.exe{Environment.NewLine}{Environment.NewLine}" +
                    $"Searched paths:{Environment.NewLine}{searchedPaths}";
                MessageBox.Show(message, "Updater not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _logger.Warning("Updater", message);
                return;
            }

            var updaterPath = updaterResolution.Path;
            var arguments = BuildUpdaterArguments();
            _logger.Info("Updater", $"Launching updater: {updaterPath}");
            ProcessService.StartProcessWithArguments(updaterPath, arguments, Path.GetDirectoryName(updaterPath));
            BeginInvoke(new Action(Close));
        }
        catch (Exception ex)
        {
            _logger.Error("Updater", "Failed to launch updater.", ex);
            MessageBox.Show(ex.Message, "Updater error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnOpenLogs_Click(object? sender, EventArgs e)
    {
        ProcessService.OpenFolder(_logger.LogDirectory);
    }

    private void btnExit_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void RefreshApplicationData()
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            EnsureConfigLoaded();
            lblTitle.Text = _config!.AppName;
            lblVersion.Text = _config.AppVersion;
            lblUpdateName.Text = _config.UpdateName;
            lblProvider.Text = _config.DatabaseProvider;

            var provider = DatabaseProviderFactory.Create(_config.DatabaseProvider);
            var database = new DatabaseService(provider, _config.ConnectionString, _logger);
            database.TestConnection();
            var dbVersion = database.GetCurrentSchemaVersion();
            var customers = database.LoadCustomers();

            lblDbVersion.Text = dbVersion;
            lblConnection.Text = "Connected";
            lblConnection.ForeColor = Color.DarkGreen;
            gridCustomers.DataSource = customers;
            _logger.Info("Startup", $"Loaded app {_config.AppVersion}, database version {dbVersion}, {customers.Rows.Count} customer rows.");
        }
        catch (Exception ex)
        {
            lblConnection.Text = "Failed";
            lblConnection.ForeColor = Color.DarkRed;
            gridCustomers.DataSource = null;
            _logger.Error("RefreshData", "Failed to load application data.", ex);
            MessageBox.Show(ex.Message, "Database or configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void EnsureConfigLoaded()
    {
        _config ??= ConfigService.Load<AppConfig>(_configPath);
    }

    private UpdaterResolution ResolveUpdaterPath(string configuredUpdaterPath)
    {
        var appDirectory = Path.GetFullPath(AppContext.BaseDirectory);
        var searchedPaths = new List<string>();
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(configuredUpdaterPath))
        {
            candidates.Add(Path.IsPathRooted(configuredUpdaterPath)
                ? configuredUpdaterPath
                : Path.Combine(appDirectory, configuredUpdaterPath));
        }

        candidates.Add(Path.Combine(appDirectory, "..", "NFOX.DemoUpdater", "NFOX.DemoUpdater.exe"));
        candidates.Add(Path.Combine(appDirectory, "NFOX.DemoUpdater.exe"));

        foreach (var sibling in GetSiblingUpdaterCandidates(appDirectory))
        {
            candidates.Add(sibling);
        }

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (searchedPaths.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            searchedPaths.Add(fullPath);
            if (File.Exists(fullPath))
            {
                return new UpdaterResolution(fullPath, searchedPaths);
            }
        }

        return new UpdaterResolution(null, searchedPaths);
    }

    private static IEnumerable<string> GetSiblingUpdaterCandidates(string appDirectory)
    {
        var current = new DirectoryInfo(appDirectory);
        while (current is not null)
        {
            var parent = current.Parent;
            if (parent is not null)
            {
                var sibling = Path.Combine(parent.FullName, "NFOX.DemoUpdater");
                yield return Path.Combine(sibling, "NFOX.DemoUpdater.exe");
                yield return Path.Combine(sibling, "bin", "Release", "net8.0-windows", "NFOX.DemoUpdater.exe");
                yield return Path.Combine(sibling, "bin", "Debug", "net8.0-windows", "NFOX.DemoUpdater.exe");
            }

            current = parent;
        }
    }

    private string[] BuildUpdaterArguments()
    {
        var appDirectory = Path.GetFullPath(AppContext.BaseDirectory);
        return new[]
        {
            "--install-dir",
            appDirectory,
            "--current-app-version",
            _config!.AppVersion,
            "--app-config-path",
            _configPath
        };
    }

    private sealed record UpdaterResolution(string? Path, List<string> SearchedPaths);
}
