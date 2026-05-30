namespace NFOX.DemoUpdater;

partial class UpdaterForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayout;
    private TextBox txtWarning;
    private Label lblCurrentVersionCaption;
    private Label lblCurrentVersion;
    private Label lblLatestVersionCaption;
    private Label lblLatestVersion;
    private Label lblCurrentDbCaption;
    private Label lblCurrentDb;
    private Label lblTargetDbCaption;
    private Label lblTargetDb;
    private Label lblUpdateNameCaption;
    private Label lblUpdateName;
    private Label lblReleaseNotesCaption;
    private TextBox txtReleaseNotes;
    private Label lblDownloadCaption;
    private ProgressBar progressDownload;
    private Label lblMigrationCaption;
    private ProgressBar progressMigration;
    private Label lblStatusCaption;
    private TextBox txtStatus;
    private FlowLayoutPanel buttonPanel;
    private Button btnCheck;
    private Button btnDownloadUpdate;
    private Button btnCancel;
    private Button btnOpenLogs;
    private Button btnClose;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        rootLayout = new TableLayoutPanel();
        txtWarning = new TextBox();
        lblCurrentVersionCaption = new Label();
        lblCurrentVersion = new Label();
        lblLatestVersionCaption = new Label();
        lblLatestVersion = new Label();
        lblCurrentDbCaption = new Label();
        lblCurrentDb = new Label();
        lblTargetDbCaption = new Label();
        lblTargetDb = new Label();
        lblUpdateNameCaption = new Label();
        lblUpdateName = new Label();
        lblReleaseNotesCaption = new Label();
        txtReleaseNotes = new TextBox();
        lblDownloadCaption = new Label();
        progressDownload = new ProgressBar();
        lblMigrationCaption = new Label();
        progressMigration = new ProgressBar();
        lblStatusCaption = new Label();
        txtStatus = new TextBox();
        buttonPanel = new FlowLayoutPanel();
        btnCheck = new Button();
        btnDownloadUpdate = new Button();
        btnCancel = new Button();
        btnOpenLogs = new Button();
        btnClose = new Button();
        rootLayout.SuspendLayout();
        buttonPanel.SuspendLayout();
        SuspendLayout();
        rootLayout.ColumnCount = 2;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(txtWarning, 0, 0);
        rootLayout.Controls.Add(lblCurrentVersionCaption, 0, 1);
        rootLayout.Controls.Add(lblCurrentVersion, 1, 1);
        rootLayout.Controls.Add(lblLatestVersionCaption, 0, 2);
        rootLayout.Controls.Add(lblLatestVersion, 1, 2);
        rootLayout.Controls.Add(lblCurrentDbCaption, 0, 3);
        rootLayout.Controls.Add(lblCurrentDb, 1, 3);
        rootLayout.Controls.Add(lblTargetDbCaption, 0, 4);
        rootLayout.Controls.Add(lblTargetDb, 1, 4);
        rootLayout.Controls.Add(lblUpdateNameCaption, 0, 5);
        rootLayout.Controls.Add(lblUpdateName, 1, 5);
        rootLayout.Controls.Add(lblReleaseNotesCaption, 0, 6);
        rootLayout.Controls.Add(txtReleaseNotes, 1, 6);
        rootLayout.Controls.Add(lblDownloadCaption, 0, 7);
        rootLayout.Controls.Add(progressDownload, 1, 7);
        rootLayout.Controls.Add(lblMigrationCaption, 0, 8);
        rootLayout.Controls.Add(progressMigration, 1, 8);
        rootLayout.Controls.Add(lblStatusCaption, 0, 9);
        rootLayout.Controls.Add(txtStatus, 1, 9);
        rootLayout.Controls.Add(buttonPanel, 0, 10);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Padding = new Padding(16);
        rootLayout.RowCount = 11;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
        rootLayout.SetColumnSpan(txtWarning, 2);
        txtWarning.BackColor = Color.FromArgb(255, 245, 225);
        txtWarning.BorderStyle = BorderStyle.FixedSingle;
        txtWarning.Dock = DockStyle.Fill;
        txtWarning.Multiline = true;
        txtWarning.ReadOnly = true;
        txtWarning.Text = "تنبيه مهم:\r\nيرجى عدم إغلاق الجهاز أو إغلاق البرنامج أو فصل الإنترنت أثناء التحديث.\r\nسيتم تحديث ملفات البرنامج وقاعدة البيانات تلقائيًا.";
        ConfigureCaption(lblCurrentVersionCaption, "Current app version");
        ConfigureValue(lblCurrentVersion);
        ConfigureCaption(lblLatestVersionCaption, "Latest app version");
        ConfigureValue(lblLatestVersion);
        ConfigureCaption(lblCurrentDbCaption, "Current DB version");
        ConfigureValue(lblCurrentDb);
        ConfigureCaption(lblTargetDbCaption, "Target DB version");
        ConfigureValue(lblTargetDb);
        ConfigureCaption(lblUpdateNameCaption, "Update name");
        ConfigureValue(lblUpdateName);
        ConfigureCaption(lblReleaseNotesCaption, "Release notes");
        txtReleaseNotes.Dock = DockStyle.Fill;
        txtReleaseNotes.Multiline = true;
        txtReleaseNotes.ReadOnly = true;
        txtReleaseNotes.ScrollBars = ScrollBars.Vertical;
        ConfigureCaption(lblDownloadCaption, "Download progress");
        progressDownload.Dock = DockStyle.Fill;
        progressDownload.Maximum = 100;
        ConfigureCaption(lblMigrationCaption, "Migration progress");
        progressMigration.Dock = DockStyle.Fill;
        progressMigration.Maximum = 100;
        ConfigureCaption(lblStatusCaption, "Status log");
        txtStatus.Dock = DockStyle.Fill;
        txtStatus.Multiline = true;
        txtStatus.ReadOnly = true;
        txtStatus.ScrollBars = ScrollBars.Vertical;
        buttonPanel.AutoSize = false;
        rootLayout.SetColumnSpan(buttonPanel, 2);
        buttonPanel.Controls.Add(btnCheck);
        buttonPanel.Controls.Add(btnDownloadUpdate);
        buttonPanel.Controls.Add(btnCancel);
        buttonPanel.Controls.Add(btnOpenLogs);
        buttonPanel.Controls.Add(btnClose);
        buttonPanel.Dock = DockStyle.Fill;
        buttonPanel.FlowDirection = FlowDirection.LeftToRight;
        ConfigureButton(btnCheck, "Check for Updates", 140);
        btnCheck.Click += btnCheck_Click;
        ConfigureButton(btnDownloadUpdate, "Update Now", 120);
        btnDownloadUpdate.Click += btnDownloadUpdate_Click;
        ConfigureButton(btnCancel, "Cancel", 100);
        btnCancel.Click += btnCancel_Click;
        ConfigureButton(btnOpenLogs, "Open Logs", 120);
        btnOpenLogs.Click += btnOpenLogs_Click;
        ConfigureButton(btnClose, "Close", 100);
        btnClose.Click += btnClose_Click;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(980, 760);
        Controls.Add(rootLayout);
        MinimumSize = new Size(850, 640);
        Name = "UpdaterForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "NFOX Demo Updater";
        Load += UpdaterForm_Load;
        rootLayout.ResumeLayout(false);
        rootLayout.PerformLayout();
        buttonPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private static void ConfigureCaption(Label label, string text)
    {
        label.Dock = DockStyle.Fill;
        label.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        label.Text = text;
        label.TextAlign = ContentAlignment.MiddleLeft;
    }

    private static void ConfigureValue(Label label)
    {
        label.Dock = DockStyle.Fill;
        label.AutoEllipsis = true;
        label.TextAlign = ContentAlignment.MiddleLeft;
    }

    private static void ConfigureButton(Button button, string text, int width)
    {
        button.Height = 34;
        button.Margin = new Padding(0, 8, 10, 8);
        button.Text = text;
        button.Width = width;
        button.UseVisualStyleBackColor = true;
    }
}
