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
    private Label lblUpdateStatus;
    private Panel updatePanel;
    private TableLayoutPanel updatePanelLayout;
    private Label lblUpdatePanelTitle;
    private TextBox txtUpdateMessage;
    private FlowLayoutPanel updateActionPanel;
    private Button btnUpdateNow;
    private Button btnUpdateLater;
    private Button btnUpdateDetails;

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
        lblUpdateStatus = new Label();
        updatePanel = new Panel();
        updatePanelLayout = new TableLayoutPanel();
        lblUpdatePanelTitle = new Label();
        txtUpdateMessage = new TextBox();
        updateActionPanel = new FlowLayoutPanel();
        btnUpdateNow = new Button();
        btnUpdateLater = new Button();
        btnUpdateDetails = new Button();
        rootLayout.SuspendLayout();
        updatePanel.SuspendLayout();
        updatePanelLayout.SuspendLayout();
        updateActionPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridCustomers).BeginInit();
        buttonPanel.SuspendLayout();
        SuspendLayout();
        rootLayout.ColumnCount = 2;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(lblTitle, 0, 0);
        rootLayout.Controls.Add(lblUpdateStatus, 0, 1);
        rootLayout.Controls.Add(updatePanel, 0, 2);
        rootLayout.Controls.Add(lblVersionCaption, 0, 3);
        rootLayout.Controls.Add(lblVersion, 1, 3);
        rootLayout.Controls.Add(lblUpdateNameCaption, 0, 4);
        rootLayout.Controls.Add(lblUpdateName, 1, 4);
        rootLayout.Controls.Add(lblProviderCaption, 0, 5);
        rootLayout.Controls.Add(lblProvider, 1, 5);
        rootLayout.Controls.Add(lblDbVersionCaption, 0, 6);
        rootLayout.Controls.Add(lblDbVersion, 1, 6);
        rootLayout.Controls.Add(lblConnectionCaption, 0, 7);
        rootLayout.Controls.Add(lblConnection, 1, 7);
        rootLayout.Controls.Add(gridCustomers, 0, 8);
        rootLayout.Controls.Add(buttonPanel, 0, 9);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Padding = new Padding(16);
        rootLayout.RowCount = 10;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
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
        rootLayout.SetColumnSpan(lblUpdateStatus, 2);
        lblUpdateStatus.Dock = DockStyle.Fill;
        lblUpdateStatus.ForeColor = Color.DimGray;
        lblUpdateStatus.Text = "Checking for updates...";
        lblUpdateStatus.TextAlign = ContentAlignment.MiddleLeft;
        updatePanel.BackColor = Color.FromArgb(255, 250, 230);
        updatePanel.BorderStyle = BorderStyle.FixedSingle;
        rootLayout.SetColumnSpan(updatePanel, 2);
        updatePanel.Controls.Add(updatePanelLayout);
        updatePanel.Dock = DockStyle.Fill;
        updatePanel.Margin = new Padding(3, 3, 3, 8);
        updatePanel.Visible = false;
        updatePanelLayout.ColumnCount = 1;
        updatePanelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        updatePanelLayout.Controls.Add(lblUpdatePanelTitle, 0, 0);
        updatePanelLayout.Controls.Add(txtUpdateMessage, 0, 1);
        updatePanelLayout.Controls.Add(updateActionPanel, 0, 2);
        updatePanelLayout.Dock = DockStyle.Fill;
        updatePanelLayout.Padding = new Padding(10);
        updatePanelLayout.RowCount = 3;
        updatePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        updatePanelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        updatePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        lblUpdatePanelTitle.Dock = DockStyle.Fill;
        lblUpdatePanelTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblUpdatePanelTitle.Text = "يوجد تحديث جديد متاح";
        lblUpdatePanelTitle.TextAlign = ContentAlignment.MiddleLeft;
        txtUpdateMessage.BorderStyle = BorderStyle.None;
        txtUpdateMessage.Dock = DockStyle.Fill;
        txtUpdateMessage.Multiline = true;
        txtUpdateMessage.ReadOnly = true;
        txtUpdateMessage.ScrollBars = ScrollBars.Vertical;
        txtUpdateMessage.BackColor = Color.FromArgb(255, 250, 230);
        updateActionPanel.Dock = DockStyle.Fill;
        updateActionPanel.FlowDirection = FlowDirection.LeftToRight;
        updateActionPanel.Controls.Add(btnUpdateNow);
        updateActionPanel.Controls.Add(btnUpdateLater);
        updateActionPanel.Controls.Add(btnUpdateDetails);
        ConfigureButton(btnUpdateNow, "تحديث الآن", 120);
        btnUpdateNow.Click += btnUpdateNow_Click;
        ConfigureButton(btnUpdateLater, "لاحقًا", 100);
        btnUpdateLater.Click += btnUpdateLater_Click;
        ConfigureButton(btnUpdateDetails, "عرض التفاصيل", 120);
        btnUpdateDetails.Click += btnUpdateDetails_Click;
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
        updateActionPanel.ResumeLayout(false);
        updatePanelLayout.ResumeLayout(false);
        updatePanelLayout.PerformLayout();
        updatePanel.ResumeLayout(false);
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
