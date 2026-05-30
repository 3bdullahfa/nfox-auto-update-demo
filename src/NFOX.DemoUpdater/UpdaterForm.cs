using NFOX.Shared.Database;
using NFOX.Shared.Models;
using NFOX.Shared.Services;

namespace NFOX.DemoUpdater;

public partial class UpdaterForm : Form
{
    private readonly Dictionary<string, string> _startupArguments;
    private readonly string _configPath;
    private readonly LogService _logger;
    private readonly DownloadService _downloader = new();
    private readonly UpdateDiscoveryService _updateDiscovery = new();
    private readonly BackupService _backupService = new();
    private UpdaterConfig? _config;
    private AppConfig? _currentAppConfig;
    private UpdateManifest? _manifest;
    private string _configDirectory = "";
    private string _installDirectory = "";
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isCriticalStage;

    public UpdaterForm(string[]? args = null)
    {
        InitializeComponent();
        _startupArguments = ParseArguments(args ?? []);
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
            if (ShouldAutoCheckOnStartup())
            {
                BeginInvoke(new Action(async () => await CheckForUpdateAsync()));
            }
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
        if (_isCriticalStage)
        {
            AppendStatus("Cancel is disabled during database migration or file replacement.");
            MessageBox.Show("لا يمكن إلغاء التحديث أثناء تحديث قاعدة البيانات أو استبدال الملفات.", "Update in progress", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

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
            await LoadManifestAsync(cancellationToken);

            var currentVersion = GetCurrentAppVersion();
            if (VersionService.IsNewer(currentVersion, _manifest!.LatestAppVersion))
            {
                AppendStatus($"Update available: {currentVersion} -> {_manifest.LatestAppVersion}");
            }
            else
            {
                AppendStatus("No update available.");
                btnDownloadUpdate.Enabled = false;
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

            if (HasPackage(_manifest.Packages.UpdatePackage))
            {
                await DownloadAndApplySinglePackageAsync(downloadDirectory, backupDirectory, cancellationToken);
            }
            else
            {
                await DownloadAndApplySeparatePackagesAsync(downloadDirectory, backupDirectory, cancellationToken);
            }
        });
    }

    private async Task DownloadAndApplySinglePackageAsync(string downloadDirectory, string backupDirectory, CancellationToken cancellationToken)
    {
        var updatePackage = _manifest!.Packages.UpdatePackage!;
        var packagePath = Path.Combine(downloadDirectory, updatePackage.FileName);

        await DownloadAndVerifyPackageAsync(updatePackage, packagePath, "update package", cancellationToken);
        CreateBackup(backupDirectory);

        var extractDirectory = Path.Combine(downloadDirectory, $"update-package-{_manifest.LatestAppVersion}");
        ZipService.ExtractToDirectory(packagePath, extractDirectory, true);
        var packageRoot = ResolveSinglePackageRoot(extractDirectory);
        var appDirectory = Path.Combine(packageRoot, "app");
        var migrationsDirectory = Path.Combine(packageRoot, "migrations");

        if (!Directory.Exists(appDirectory))
        {
            throw new DirectoryNotFoundException($"Update package does not contain an app folder: {appDirectory}");
        }

        if (!Directory.Exists(migrationsDirectory))
        {
            throw new DirectoryNotFoundException($"Update package does not contain a migrations folder: {migrationsDirectory}");
        }

        await ApplyMigrationsAsync(migrationsDirectory, cancellationToken);
        WaitForMainAppToExit();
        ReplaceAndLaunchUpdatedApp(appDirectory);
    }

    private async Task DownloadAndApplySeparatePackagesAsync(string downloadDirectory, string backupDirectory, CancellationToken cancellationToken)
    {
        var appPackage = _manifest!.Packages.App;
        var migrationPackage = _manifest.Packages.Migrations;
        var appPackagePath = Path.Combine(downloadDirectory, appPackage.FileName);
        var migrationPackagePath = Path.Combine(downloadDirectory, migrationPackage.FileName);

        await DownloadAndVerifyPackageAsync(appPackage, appPackagePath, "app package", cancellationToken);
        await DownloadAndVerifyPackageAsync(migrationPackage, migrationPackagePath, "migration package", cancellationToken);
        CreateBackup(backupDirectory);

        var migrationsExtractDirectory = Path.Combine(downloadDirectory, $"migrations-{_manifest.LatestAppVersion}");
        ZipService.ExtractToDirectory(migrationPackagePath, migrationsExtractDirectory, true);
        await ApplyMigrationsAsync(migrationsExtractDirectory, cancellationToken);
        WaitForMainAppToExit();

        var appExtractDirectory = Path.Combine(downloadDirectory, $"app-{_manifest.LatestAppVersion}");
        ZipService.ExtractToDirectory(appPackagePath, appExtractDirectory, true);
        ReplaceAndLaunchUpdatedApp(appExtractDirectory);
    }

    private async Task DownloadAndVerifyPackageAsync(PackageInfo package, string packagePath, string label, CancellationToken cancellationToken)
    {
        ValidatePackage(package, label);
        AppendStatus($"Downloading {label}.");
        await _downloader.DownloadFileAsync(package.DownloadUrl, packagePath, new Progress<double>(SetDownloadProgress), cancellationToken);
        AppendStatus($"Verifying {label} SHA256.");
        FileHashService.VerifySha256(packagePath, package.Sha256);
        AppendStatus($"{label} hash verified.");
    }

    private void CreateBackup(string backupDirectory)
    {
        AppendStatus("Creating backup.");
        var backupPath = _backupService.CreateBackup(_installDirectory, backupDirectory);
        AppendStatus($"Backup created: {backupPath}");
    }

    private async Task ApplyMigrationsAsync(string migrationsDirectory, CancellationToken cancellationToken)
    {
        AppendStatus("Applying database migrations.");
        EnterCriticalStage();

        var provider = DatabaseProviderFactory.Create(_config!.DatabaseProvider);
        var migrationLogger = new LogService("migration", _logger.LogDirectory);
        var runner = new MigrationRunner(provider, _config.ConnectionString, migrationLogger);
        var migrationResult = await Task.Run(() => runner.RunMigrations(migrationsDirectory, _manifest!.LatestAppVersion), cancellationToken);
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
    }

    private static string ResolveSinglePackageRoot(string extractDirectory)
    {
        if (Directory.Exists(Path.Combine(extractDirectory, "app")))
        {
            return extractDirectory;
        }

        var candidates = Directory.GetDirectories(extractDirectory)
            .Where(directory => Directory.Exists(Path.Combine(directory, "app")))
            .OrderBy(directory => directory, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (candidates.Count == 1)
        {
            return candidates[0];
        }

        throw new DirectoryNotFoundException($"Update package does not contain the expected app folder under: {extractDirectory}");
    }

    private void WaitForMainAppToExit()
    {
        if (!ProcessService.WaitForProcessExit("NFOX.DemoApp.exe", TimeSpan.FromSeconds(5)))
        {
            throw new InvalidOperationException("NFOX.DemoApp.exe is still running. Close the application and run the updater again.");
        }
    }

    private void ReplaceAndLaunchUpdatedApp(string appDirectory)
    {
        AppendStatus("Replacing application files.");
        ReplaceApplicationFiles(appDirectory);

        AppendStatus("Launching updated application.");
        var appExe = Path.Combine(_installDirectory, "NFOX.DemoApp.exe");
        ProcessService.StartProcess(appExe, null, _installDirectory);
        AppendStatus("Update completed successfully.");
    }

    private static bool HasPackage(PackageInfo? package)
    {
        return package is not null &&
            !string.IsNullOrWhiteSpace(package.FileName) &&
            !string.IsNullOrWhiteSpace(package.DownloadUrl) &&
            !string.IsNullOrWhiteSpace(package.Sha256);
    }

    private static void ValidatePackage(PackageInfo package, string label)
    {
        if (!HasPackage(package))
        {
            throw new InvalidOperationException($"Manifest {label} entry is incomplete.");
        }
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
        AppendStatus(_config!.UpdateSource.Equals("GitHub", StringComparison.OrdinalIgnoreCase)
            ? $"Checking GitHub latest release for {_config.GitHubOwner}/{_config.GitHubRepo}."
            : $"Loading manifest from {_config.ManifestUrl}");
        _manifest = await _updateDiscovery.GetManifestAsync(_config, cancellationToken);

        lblLatestVersion.Text = _manifest.LatestAppVersion;
        lblTargetDb.Text = _manifest.TargetDbVersion;
        lblUpdateName.Text = _manifest.UpdateName;
        txtReleaseNotes.Text = BuildManifestDetails(_manifest);
        btnDownloadUpdate.Enabled = VersionService.IsNewer(GetCurrentAppVersion(), _manifest.LatestAppVersion);
    }

    private async Task RunUiOperationAsync(Func<CancellationToken, Task> operation)
    {
        SetBusy(true);
        _isCriticalStage = false;
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
            _isCriticalStage = false;
            SetBusy(false);
        }
    }

    private void LoadLocalState()
    {
        _config = ConfigService.Load<UpdaterConfig>(_configPath);
        ApplyStartupArguments(_config);
        _installDirectory = ConfigService.ResolvePath(_configDirectory, _config.InstallDirectory);
        var appConfigPathOverride = GetArgumentValue("app-config-path") ?? GetArgumentValue("config-path");
        var appConfigPath = string.IsNullOrWhiteSpace(appConfigPathOverride)
            ? Path.Combine(_installDirectory, "appsettings.json")
            : ConfigService.ResolvePath(_configDirectory, appConfigPathOverride);
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
        return _currentAppConfig?.AppVersion ?? GetArgumentValue("current-app-version") ?? _config?.CurrentAppVersion ?? "0.0.0";
    }

    private void ApplyStartupArguments(UpdaterConfig config)
    {
        var installDirectory = GetArgumentValue("install-dir");
        if (!string.IsNullOrWhiteSpace(installDirectory))
        {
            config.InstallDirectory = installDirectory;
        }

        var currentVersion = GetArgumentValue("current-app-version");
        if (!string.IsNullOrWhiteSpace(currentVersion))
        {
            config.CurrentAppVersion = currentVersion;
        }

        var updateSource = GetArgumentValue("update-source");
        if (!string.IsNullOrWhiteSpace(updateSource))
        {
            config.UpdateSource = updateSource;
        }

        var githubOwner = GetArgumentValue("github-owner");
        if (!string.IsNullOrWhiteSpace(githubOwner))
        {
            config.GitHubOwner = githubOwner;
        }

        var githubRepo = GetArgumentValue("github-repo");
        if (!string.IsNullOrWhiteSpace(githubRepo))
        {
            config.GitHubRepo = githubRepo;
        }

        var manifestUrl = GetArgumentValue("manifest-url");
        if (!string.IsNullOrWhiteSpace(manifestUrl))
        {
            config.ManifestUrl = manifestUrl;
        }

        var useLatestRelease = GetArgumentValue("github-use-latest-release");
        if (bool.TryParse(useLatestRelease, out var parsedUseLatestRelease))
        {
            config.GitHubUseLatestRelease = parsedUseLatestRelease;
        }
    }

    private string? GetArgumentValue(string name)
    {
        return _startupArguments.TryGetValue(name, out var value) ? value : null;
    }

    private static Dictionary<string, string> ParseArguments(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            var token = args[i];
            if (!token.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            var keyValue = token[2..].Split('=', 2);
            var key = keyValue[0].Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (keyValue.Length == 2)
            {
                result[key] = keyValue[1];
                continue;
            }

            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                result[key] = args[++i];
            }
            else
            {
                result[key] = "true";
            }
        }

        return result;
    }

    private void SetDownloadProgress(double value)
    {
        progressDownload.Value = Math.Max(0, Math.Min(100, (int)Math.Round(value)));
    }

    private void SetBusy(bool busy)
    {
        btnCheck.Enabled = !busy;
        btnDownloadUpdate.Enabled = !busy && IsUpdateAvailable();
        btnCancel.Enabled = busy && !_isCriticalStage;
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
        var userMessage = exception is HttpRequestException
            ? "تعذر الاتصال بخادم التحديثات. لم يتم استبدال ملفات البرنامج ويمكنك استخدام الإصدار الحالي."
            : exception.Message;
        AppendStatus($"ERROR: {userMessage}");
        _logger.Error(operation, exception.Message, exception);
        MessageBox.Show(userMessage, "Updater error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private bool ShouldAutoCheckOnStartup()
    {
        return GetArgumentValue("auto-check")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true ||
            _config?.UpdateSource.Equals("GitHub", StringComparison.OrdinalIgnoreCase) == true;
    }

    private bool IsUpdateAvailable()
    {
        return _manifest is not null && VersionService.IsNewer(GetCurrentAppVersion(), _manifest.LatestAppVersion);
    }

    private void EnterCriticalStage()
    {
        _isCriticalStage = true;
        btnCancel.Enabled = false;
    }

    private static string BuildManifestDetails(UpdateManifest manifest)
    {
        var required = manifest.IsRequired ? "Yes" : "No";
        return
            $"Update name: {manifest.UpdateName}{Environment.NewLine}" +
            $"Release notes: {manifest.ReleaseNotes}{Environment.NewLine}" +
            $"Target DB version: {manifest.TargetDbVersion}{Environment.NewLine}" +
            $"Required: {required}{Environment.NewLine}{Environment.NewLine}" +
            "تنبيه مهم:" + Environment.NewLine +
            "يرجى عدم إغلاق الجهاز أو إغلاق البرنامج أو فصل الإنترنت أثناء التحديث." + Environment.NewLine +
            "سيتم تحديث ملفات البرنامج وقاعدة البيانات تلقائيًا.";
    }
}
