namespace NFOX.DemoApp;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayout;
    private Label lblTitle;
    private Label lblVersionCaption;
    private Label lblVersion;
    private Label lblUpdateNameCaption;
    private Label lblUpdateName;
    private Label lblProviderCaption;
    private Label lblProvider;
    private Label lblDbVersionCaption;
    private Label lblDbVersion;
    private Label lblConnectionCaption;
    private Label lblConnection;
    private DataGridView gridCustomers;
    private FlowLayoutPanel buttonPanel;
    private Button btnCheckForUpdate;
    private Button btnRefresh;
    private Button btnOpenLogs;
    private Button btnExit;

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
        lblTitle = new Label();
        lblVersionCaption = new Label();
        lblVersion = new Label();
        lblUpdateNameCaption = new Label();
        lblUpdateName = new Label();
        lblProviderCaption = new Label();
        lblProvider = new Label();
        lblDbVersionCaption = new Label();
        lblDbVersion = new Label();
        lblConnectionCaption = new Label();
        lblConnection = new Label();
        gridCustomers = new DataGridView();
        buttonPanel = new FlowLayoutPanel();
        btnCheckForUpdate = new Button();
        btnRefresh = new Button();
        btnOpenLogs = new Button();
        btnExit = new Button();
        rootLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridCustomers).BeginInit();
        buttonPanel.SuspendLayout();
        SuspendLayout();
        rootLayout.ColumnCount = 2;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(lblTitle, 0, 0);
        rootLayout.Controls.Add(lblVersionCaption, 0, 1);
        rootLayout.Controls.Add(lblVersion, 1, 1);
        rootLayout.Controls.Add(lblUpdateNameCaption, 0, 2);
        rootLayout.Controls.Add(lblUpdateName, 1, 2);
        rootLayout.Controls.Add(lblProviderCaption, 0, 3);
        rootLayout.Controls.Add(lblProvider, 1, 3);
        rootLayout.Controls.Add(lblDbVersionCaption, 0, 4);
        rootLayout.Controls.Add(lblDbVersion, 1, 4);
        rootLayout.Controls.Add(lblConnectionCaption, 0, 5);
        rootLayout.Controls.Add(lblConnection, 1, 5);
        rootLayout.Controls.Add(gridCustomers, 0, 6);
        rootLayout.Controls.Add(buttonPanel, 0, 7);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Padding = new Padding(16);
        rootLayout.RowCount = 8;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        lblTitle.AutoSize = true;
        rootLayout.SetColumnSpan(lblTitle, 2);
        lblTitle.Dock = DockStyle.Fill;
        lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        lblTitle.Text = "NFOX ERP Demo";
        lblTitle.TextAlign = ContentAlignment.MiddleLeft;
        ConfigureCaption(lblVersionCaption, "App version");
        ConfigureValue(lblVersion);
        ConfigureCaption(lblUpdateNameCaption, "Update name");
        ConfigureValue(lblUpdateName);
        ConfigureCaption(lblProviderCaption, "Database provider");
        ConfigureValue(lblProvider);
        ConfigureCaption(lblDbVersionCaption, "Database version");
        ConfigureValue(lblDbVersion);
        ConfigureCaption(lblConnectionCaption, "Connection status");
        ConfigureValue(lblConnection);
        gridCustomers.AllowUserToAddRows = false;
        gridCustomers.AllowUserToDeleteRows = false;
        gridCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridCustomers.BackgroundColor = Color.White;
        gridCustomers.BorderStyle = BorderStyle.Fixed3D;
        rootLayout.SetColumnSpan(gridCustomers, 2);
        gridCustomers.Dock = DockStyle.Fill;
        gridCustomers.ReadOnly = true;
        gridCustomers.RowHeadersWidth = 48;
        buttonPanel.AutoSize = false;
        rootLayout.SetColumnSpan(buttonPanel, 2);
        buttonPanel.Controls.Add(btnCheckForUpdate);
        buttonPanel.Controls.Add(btnRefresh);
        buttonPanel.Controls.Add(btnOpenLogs);
        buttonPanel.Controls.Add(btnExit);
        buttonPanel.Dock = DockStyle.Fill;
        buttonPanel.FlowDirection = FlowDirection.LeftToRight;
        ConfigureButton(btnCheckForUpdate, "Check for Update", 150);
        btnCheckForUpdate.Click += btnCheckForUpdate_Click;
        ConfigureButton(btnRefresh, "Refresh Data", 120);
        btnRefresh.Click += btnRefresh_Click;
        ConfigureButton(btnOpenLogs, "Open Logs Folder", 150);
        btnOpenLogs.Click += btnOpenLogs_Click;
        ConfigureButton(btnExit, "Exit", 90);
        btnExit.Click += btnExit_Click;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(980, 640);
        Controls.Add(rootLayout);
        MinimumSize = new Size(820, 520);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "NFOX Auto Update Demo";
        Load += MainForm_Load;
        rootLayout.ResumeLayout(false);
        rootLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridCustomers).EndInit();
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
        button.Margin = new Padding(0, 6, 10, 6);
        button.Text = text;
        button.Width = width;
        button.UseVisualStyleBackColor = true;
    }
}
