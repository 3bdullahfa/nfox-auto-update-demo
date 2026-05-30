using System.Text.Json;
using NFOX.Shared.Database;
using NFOX.Shared.Models;
using NFOX.Shared.Services;

namespace NFOX.DemoUpdater;

public partial class UpdaterForm : Form
{
    private readonly string _configPath;
    private readonly LogService _logger;
    private readonly DownloadService _downloader = new();
    private readonly BackupService _backupService = new();
    private UpdaterConfig? _config;
    private AppConfig? _currentAppConfig;
    private UpdateManifest? _manifest;
    private string _configDirectory = "";
    private string _installDirectory = "";
    private CancellationTokenSource? _cancellationTokenSource;

    public UpdaterForm()
    {
        InitializeComponent();
        _configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        _configDirectory = Path.GetDirectoryName(_configPath) ?? AppContext.BaseDirectory;
        _logger = new LogService("updater");
    }

    private void UpdaterForm_Load(object? sender, EventArgs e)
    {
        try
        {
            LoadLocalState();
            AppendStatus("Updater loaded.");
        }
        catch (Exception ex)
        {
            HandleError("Load", ex);
        }
    }

    private async void btnCheck_Click(object? sender, EventArgs e)
    {
        await CheckForUpdateAsync();
    }

    private async void btnDownloadUpdate_Click(object? sender, EventArgs e)
    {
        await DownloadAndUpdateAsync();
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        AppendStatus("Cancellation requested.");
    }

    private void btnOpenLogs_Click(object? sender, EventArgs e)
    {
        ProcessService.OpenFolder(_logger.LogDirectory);
    }

    private void btnClose_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private async Task CheckForUpdateAsync()
    {
        await RunUiOperationAsync(async cancellationToken =>
        {
            LoadLocalState();
            if (_config!.ManifestUrl.Contains("PUT_GITHUB_RELEASE_MANIFEST_URL_HERE", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("ManifestUrl is still a placeholder. Set it to a file:/// URL or a GitHub release manifest URL.");
            }

            await LoadManifestAsync(cancellationToken);

            var currentVersion = GetCurrentAppVersion();
            if (VersionService.IsNewer(currentVersion, _manifest!.LatestAppVersion))
            {
                AppendStatus($"Update available: {currentVersion} -> {_manifest.LatestAppVersion}");
            }
            else
            {
                AppendStatus("No update available.");
            }
        });
    }

    private async Task DownloadAndUpdateAsync()
    {
        await RunUiOperationAsync(async cancellationToken =>
        {
            if (_manifest is null)
            {
                LoadLocalState();
                await LoadManifestAsync(cancellationToken);
            }

            if (_manifest is null)
            {
                throw new InvalidOperationException("No manifest is loaded.");
            }

            var currentVersion = GetCurrentAppVersion();
            if (!VersionService.IsNewer(currentVersion, _manifest.LatestAppVersion))
            {
                AppendStatus("No update was applied because the local version is current.");
                return;
            }

            var downloadDirectory = ConfigService.ResolvePath(_configDirectory, _config!.DownloadDirectory);
            var backupDirectory = ConfigService.ResolvePath(_configDirectory, _config.BackupDirectory);
            Directory.CreateDirectory(downloadDirectory);
            Directory.CreateDirectory(backupDirectory);

            var appPackagePath = Path.Combine(downloadDirectory, _manifest.Packages.App.FileName);
            var migrationPackagePath = Path.Combine(downloadDirectory, _manifest.Packages.Migrations.FileName);

            AppendStatus("Downloading app package.");
            await _downloader.DownloadFileAsync(_manifest.Packages.App.DownloadUrl, appPackagePath, new Progress<double>(SetDownloadProgress), cancellationToken);
            FileHashService.VerifySha256(appPackagePath, _manifest.Packages.App.Sha256);
            AppendStatus("App package hash verified.");

            AppendStatus("Downloading migration package.");
            await _downloader.DownloadFileAsync(_manifest.Packages.Migrations.DownloadUrl, migrationPackagePath, new Progress<double>(SetDownloadProgress), cancellationToken);
            FileHashService.VerifySha256(migrationPackagePath, _manifest.Packages.Migrations.Sha256);
            AppendStatus("Migration package hash verified.");

            var backupPath = _backupService.CreateBackup(_installDirectory, backupDirectory);
            AppendStatus($"Backup created: {backupPath}");

            var migrationsExtractDirectory = Path.Combine(downloadDirectory, $"migrations-{_manifest.LatestAppVersion}");
            ZipService.ExtractToDirectory(migrationPackagePath, migrationsExtractDirectory, true);
            AppendStatus("Applying database migrations.");

            var provider = DatabaseProviderFactory.Create(_config.DatabaseProvider);
            var migrationLogger = new LogService("migration", _logger.LogDirectory);
            var runner = new MigrationRunner(provider, _config.ConnectionString, migrationLogger);
            var migrationResult = await Task.Run(() => runner.RunMigrations(migrationsExtractDirectory, _manifest.LatestAppVersion), cancellationToken);
            progressMigration.Value = migrationResult.Success ? 100 : 0;
            foreach (var migration in migrationResult.Migrations)
            {
                AppendStatus($"{migration.Status}: {migration.ScriptName} - {migration.Message}");
            }

            if (!migrationResult.Success)
            {
                throw new InvalidOperationException(migrationResult.Message);
            }

            AppendStatus("Database migrations completed.");
            if (!ProcessService.WaitForProcessExit("NFOX.DemoApp.exe", TimeSpan.FromSeconds(5)))
            {
                throw new InvalidOperationException("NFOX.DemoApp.exe is still running. Close the application and run the updater again.");
            }

            var appExtractDirectory = Path.Combine(downloadDirectory, $"app-{_manifest.LatestAppVersion}");
            ZipService.ExtractToDirectory(appPackagePath, appExtractDirectory, true);
            AppendStatus("Replacing application files.");
            ReplaceApplicationFiles(appExtractDirectory);

            AppendStatus("Launching updated application.");
            var appExe = Path.Combine(_installDirectory, "NFOX.DemoApp.exe");
            ProcessService.StartProcess(appExe, null, _installDirectory);
            AppendStatus("Update completed successfully.");
        });
    }

    private void ReplaceApplicationFiles(string extractedAppDirectory)
    {
        var appSettingsPath = Path.Combine(_installDirectory, "appsettings.json");
        var previousConnectionString = _currentAppConfig?.ConnectionString ?? _config!.ConnectionString;
        var previousProvider = _currentAppConfig?.DatabaseProvider ?? _config!.DatabaseProvider;
        var previousUpdaterPath = _currentAppConfig?.UpdaterPath ?? "../NFOX.DemoUpdater/NFOX.DemoUpdater.exe";

        ZipService.CopyDirectory(extractedAppDirectory, _installDirectory, true);

        var updatedAppConfig = ConfigService.Load<AppConfig>(appSettingsPath);
        updatedAppConfig.ConnectionString = previousConnectionString;
        updatedAppConfig.DatabaseProvider = previousProvider;
        updatedAppConfig.UpdaterPath = previousUpdaterPath;
        updatedAppConfig.AppVersion = _manifest!.LatestAppVersion;
        updatedAppConfig.UpdateName = _manifest.UpdateName;
        ConfigService.Save(appSettingsPath, updatedAppConfig);

        _config!.CurrentAppVersion = _manifest.LatestAppVersion;
        ConfigService.Save(_configPath, _config);
    }

    private async Task LoadManifestAsync(CancellationToken cancellationToken)
    {
        if (_config!.ManifestUrl.Contains("PUT_GITHUB_RELEASE_MANIFEST_URL_HERE", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("ManifestUrl is still a placeholder. Set it to a file:/// URL or a GitHub release manifest URL.");
        }

        AppendStatus($"Loading manifest from {_config!.ManifestUrl}");
        var manifestJson = await _downloader.GetStringAsync(_config.ManifestUrl, cancellationToken);
        _manifest = JsonSerializer.Deserialize<UpdateManifest>(manifestJson, ConfigService.JsonOptions)
            ?? throw new InvalidOperationException("Manifest JSON is empty or invalid.");

        lblLatestVersion.Text = _manifest.LatestAppVersion;
        lblTargetDb.Text = _manifest.TargetDbVersion;
        lblUpdateName.Text = _manifest.UpdateName;
        txtReleaseNotes.Text = _manifest.ReleaseNotes;
    }

    private async Task RunUiOperationAsync(Func<CancellationToken, Task> operation)
    {
        SetBusy(true);
        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            await operation(_cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            AppendStatus("Operation cancelled.");
        }
        catch (Exception ex)
        {
            HandleError("Updater", ex);
        }
        finally
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            SetBusy(false);
        }
    }

    private void LoadLocalState()
    {
        _config = ConfigService.Load<UpdaterConfig>(_configPath);
        _installDirectory = ConfigService.ResolvePath(_configDirectory, _config.InstallDirectory);
        var appConfigPath = Path.Combine(_installDirectory, "appsettings.json");
        _currentAppConfig = File.Exists(appConfigPath) ? ConfigService.Load<AppConfig>(appConfigPath) : null;
        lblCurrentVersion.Text = GetCurrentAppVersion();
        lblLatestVersion.Text = _manifest?.LatestAppVersion ?? "-";
        lblTargetDb.Text = _manifest?.TargetDbVersion ?? "-";
        lblUpdateName.Text = _manifest?.UpdateName ?? "-";
        txtReleaseNotes.Text = _manifest?.ReleaseNotes ?? "";

        try
        {
            var provider = DatabaseProviderFactory.Create(_config.DatabaseProvider);
            var database = new DatabaseService(provider, _config.ConnectionString, _logger);
            lblCurrentDb.Text = database.GetCurrentSchemaVersion();
        }
        catch (Exception ex)
        {
            lblCurrentDb.Text = "Unavailable";
            _logger.Error("Database", "Failed to read current database version.", ex);
        }
    }

    private string GetCurrentAppVersion()
    {
        return _currentAppConfig?.AppVersion ?? _config?.CurrentAppVersion ?? "0.0.0";
    }

    private void SetDownloadProgress(double value)
    {
        progressDownload.Value = Math.Max(0, Math.Min(100, (int)Math.Round(value)));
    }

    private void SetBusy(bool busy)
    {
        btnCheck.Enabled = !busy;
        btnDownloadUpdate.Enabled = !busy;
        btnCancel.Enabled = busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private void AppendStatus(string message)
    {
        var line = $"{DateTime.Now:HH:mm:ss}  {message}";
        txtStatus.AppendText(line + Environment.NewLine);
        _logger.Info("Status", message);
    }

    private void HandleError(string operation, Exception exception)
    {
        AppendStatus($"ERROR: {exception.Message}");
        _logger.Error(operation, exception.Message, exception);
        MessageBox.Show(exception.Message, "Updater error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
