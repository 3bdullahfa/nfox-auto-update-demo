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
            var configDirectory = Path.GetDirectoryName(_configPath) ?? AppContext.BaseDirectory;
            var updaterPath = ConfigService.ResolvePath(configDirectory, _config!.UpdaterPath);
            if (!File.Exists(updaterPath))
            {
                MessageBox.Show($"Updater executable was not found:{Environment.NewLine}{updaterPath}", "Updater not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _logger.Warning("Updater", $"Updater executable was not found: {updaterPath}");
                return;
            }

            _logger.Info("Updater", $"Launching updater: {updaterPath}");
            ProcessService.StartProcess(updaterPath, null, Path.GetDirectoryName(updaterPath));
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
}
