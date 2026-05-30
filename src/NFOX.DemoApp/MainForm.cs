using NFOX.Shared.Database;
using NFOX.Shared.Models;
using NFOX.Shared.Services;

namespace NFOX.DemoApp;

public partial class MainForm : Form
{
    private readonly string _configPath;
    private readonly LogService _logger;
    private readonly UpdateDiscoveryService _updateDiscovery = new();
    private AppConfig? _config;
    private UpdateManifest? _availableUpdate;

    public MainForm()
    {
        InitializeComponent();
        _configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        _logger = new LogService("app");
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        RefreshApplicationData();
        if (_config?.AutoCheckForUpdatesOnStartup == true)
        {
            await CheckForUpdatesAsync(_config.ShowNoUpdateMessage);
        }
    }

    private async void btnRefresh_Click(object? sender, EventArgs e)
    {
        RefreshApplicationData();
        if (_config?.AutoCheckForUpdatesOnStartup == true)
        {
            await CheckForUpdatesAsync(false);
        }
    }

    private async void btnCheckForUpdate_Click(object? sender, EventArgs e)
    {
        await CheckForUpdatesAsync(true);
    }

    private void btnUpdateNow_Click(object? sender, EventArgs e)
    {
        LaunchUpdater();
    }

    private void btnUpdateLater_Click(object? sender, EventArgs e)
    {
        if (_availableUpdate?.IsRequired == true)
        {
            MessageBox.Show("يوجد تحديث إجباري. يجب تثبيت التحديث قبل متابعة استخدام النظام.", "Required update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ShowUpdatePanel(false);
        lblUpdateStatus.Text = "تم تأجيل التحديث";
    }

    private void btnUpdateDetails_Click(object? sender, EventArgs e)
    {
        if (_availableUpdate is null)
        {
            return;
        }

        MessageBox.Show(BuildUpdateMessage(_availableUpdate), "Update details", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void LaunchUpdater()
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

    private async Task CheckForUpdatesAsync(bool showNoUpdateMessage)
    {
        try
        {
            EnsureConfigLoaded();
            lblUpdateStatus.ForeColor = Color.DimGray;
            lblUpdateStatus.Text = "Checking GitHub for updates...";
            var manifest = await _updateDiscovery.GetManifestAsync(_config!, CancellationToken.None);
            if (VersionService.IsNewer(_config!.AppVersion, manifest.LatestAppVersion))
            {
                _availableUpdate = manifest;
                lblUpdateStatus.ForeColor = manifest.IsRequired ? Color.DarkRed : Color.DarkOrange;
                lblUpdateStatus.Text = manifest.IsRequired
                    ? "يوجد تحديث إجباري متاح"
                    : "يوجد تحديث جديد متاح";
                ShowUpdateNotification(manifest);
                _logger.Info("UpdateCheck", $"Update available: {_config.AppVersion} -> {manifest.LatestAppVersion}");
                return;
            }

            _availableUpdate = null;
            ShowUpdatePanel(false);
            lblUpdateStatus.ForeColor = Color.DarkGreen;
            lblUpdateStatus.Text = "النظام محدث";
            _logger.Info("UpdateCheck", $"System is up to date. Local version {_config.AppVersion}, latest {manifest.LatestAppVersion}.");
            if (showNoUpdateMessage)
            {
                MessageBox.Show("النظام محدث ولا يوجد تحديث جديد.", "Update status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            _logger.Error("UpdateCheck", "Failed to check GitHub for updates.", ex);
            ShowUpdatePanel(false);
            lblUpdateStatus.ForeColor = Color.DarkRed;
            lblUpdateStatus.Text = "تعذر الاتصال بخادم التحديثات. يمكنك استخدام النظام بالإصدار الحالي.";
        }
    }

    private void ShowUpdateNotification(UpdateManifest manifest)
    {
        lblUpdatePanelTitle.Text = manifest.IsRequired
            ? "يوجد تحديث إجباري"
            : "يوجد تحديث جديد متاح";
        txtUpdateMessage.Text = BuildUpdateMessage(manifest);
        btnUpdateLater.Enabled = !manifest.IsRequired;
        btnRefresh.Enabled = !manifest.IsRequired;
        gridCustomers.Enabled = !manifest.IsRequired;
        ShowUpdatePanel(true);
    }

    private void ShowUpdatePanel(bool show)
    {
        updatePanel.Visible = show;
        rootLayout.RowStyles[2].Height = show ? 168F : 0F;
    }

    private string BuildUpdateMessage(UpdateManifest manifest)
    {
        var requiredMessage = manifest.IsRequired
            ? $"{Environment.NewLine}هذا التحديث إجباري ولا يمكن تشغيل النظام قبل تثبيته.{Environment.NewLine}"
            : "";
        return
            $"يوجد تحديث جديد لنظام NFOX ERP{Environment.NewLine}{Environment.NewLine}" +
            $"الإصدار الحالي: {_config!.AppVersion}{Environment.NewLine}" +
            $"الإصدار الجديد: {manifest.LatestAppVersion}{Environment.NewLine}" +
            $"اسم التحديث: {manifest.UpdateName}{Environment.NewLine}" +
            $"إصدار قاعدة البيانات المستهدف: {manifest.TargetDbVersion}{Environment.NewLine}" +
            $"ملاحظات الإصدار: {manifest.ReleaseNotes}{Environment.NewLine}" +
            requiredMessage +
            $"{Environment.NewLine}سيتم تحديث ملفات البرنامج وقاعدة البيانات تلقائيًا.{Environment.NewLine}" +
            "يرجى عدم إغلاق الجهاز أو فصل الإنترنت أثناء عملية التحديث.";
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
            _configPath,
            "--update-source",
            _config.UpdateSource,
            "--github-owner",
            _config.GitHubOwner,
            "--github-repo",
            _config.GitHubRepo,
            "--github-use-latest-release",
            _config.GitHubUseLatestRelease.ToString().ToLowerInvariant(),
            "--manifest-url",
            _config.ManifestUrl,
            "--auto-check",
            "true"
        };
    }

    private sealed record UpdaterResolution(string? Path, List<string> SearchedPaths);
}
