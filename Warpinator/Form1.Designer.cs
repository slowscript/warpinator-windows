
namespace Warpinator
{
    partial class Form1
    {
        /// <summary>
        /// Vyžaduje se proměnná návrháře.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Uvolněte všechny používané prostředky.
        /// </summary>
        /// <param name="disposing">hodnota true, když by se měl spravovaný prostředek odstranit; jinak false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kód generovaný Návrhářem Windows Form

        /// <summary>
        /// Metoda vyžadovaná pro podporu Návrháře - neupravovat
        /// obsah této metody v editoru kódu.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblIP = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manualConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rescanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reannounceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionIssuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitHubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblDevices = new System.Windows.Forms.Label();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifyIconMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.btnRescan = new System.Windows.Forms.Button();
            this.lblNoDevicesFound = new System.Windows.Forms.Label();
            this.lblInitializing = new System.Windows.Forms.Label();
            this.btnConnectManually = new System.Windows.Forms.Button();
            this.btnCancelShare = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.notifyIconMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblIP,
            this.lblStatus});
            this.statusStrip1.Name = "statusStrip1";
            // 
            // lblIP
            // 
            resources.ApplyResources(this.lblIP, "lblIP");
            this.lblIP.Name = "lblIP";
            // 
            // lblStatus
            // 
            resources.ApplyResources(this.lblStatus, "lblStatus");
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Spring = true;
            // 
            // menuStrip1
            // 
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manualConnectionToolStripMenuItem,
            this.rescanToolStripMenuItem,
            this.reannounceToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            // 
            // manualConnectionToolStripMenuItem
            // 
            resources.ApplyResources(this.manualConnectionToolStripMenuItem, "manualConnectionToolStripMenuItem");
            this.manualConnectionToolStripMenuItem.Name = "manualConnectionToolStripMenuItem";
            this.manualConnectionToolStripMenuItem.Click += new System.EventHandler(this.ManualConnection_Click);
            // 
            // rescanToolStripMenuItem
            // 
            resources.ApplyResources(this.rescanToolStripMenuItem, "rescanToolStripMenuItem");
            this.rescanToolStripMenuItem.Name = "rescanToolStripMenuItem";
            this.rescanToolStripMenuItem.Click += new System.EventHandler(this.RescanToolStripMenuItem_Click);
            // 
            // reannounceToolStripMenuItem
            // 
            resources.ApplyResources(this.reannounceToolStripMenuItem, "reannounceToolStripMenuItem");
            this.reannounceToolStripMenuItem.Name = "reannounceToolStripMenuItem";
            this.reannounceToolStripMenuItem.Click += new System.EventHandler(this.ReannounceToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            resources.ApplyResources(this.settingsToolStripMenuItem, "settingsToolStripMenuItem");
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            resources.ApplyResources(this.quitToolStripMenuItem, "quitToolStripMenuItem");
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.QuitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.connectionIssuesToolStripMenuItem,
            this.gitHubToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            // 
            // aboutToolStripMenuItem
            // 
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // connectionIssuesToolStripMenuItem
            // 
            resources.ApplyResources(this.connectionIssuesToolStripMenuItem, "connectionIssuesToolStripMenuItem");
            this.connectionIssuesToolStripMenuItem.Name = "connectionIssuesToolStripMenuItem";
            this.connectionIssuesToolStripMenuItem.Click += new System.EventHandler(this.ConnectionIssuesToolStripMenuItem_Click);
            // 
            // gitHubToolStripMenuItem
            // 
            resources.ApplyResources(this.gitHubToolStripMenuItem, "gitHubToolStripMenuItem");
            this.gitHubToolStripMenuItem.Name = "gitHubToolStripMenuItem";
            this.gitHubToolStripMenuItem.Click += new System.EventHandler(this.GitHubToolStripMenuItem_Click);
            // 
            // lblDevices
            // 
            resources.ApplyResources(this.lblDevices, "lblDevices");
            this.lblDevices.Name = "lblDevices";
            // 
            // flowLayoutPanel
            // 
            resources.ApplyResources(this.flowLayoutPanel, "flowLayoutPanel");
            this.flowLayoutPanel.BackColor = System.Drawing.SystemColors.Control;
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            // 
            // notifyIcon
            // 
            resources.ApplyResources(this.notifyIcon, "notifyIcon");
            this.notifyIcon.ContextMenuStrip = this.notifyIconMenu;
            // 
            // notifyIconMenu
            // 
            resources.ApplyResources(this.notifyIconMenu, "notifyIconMenu");
            this.notifyIconMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.notifyIconMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.quitToolStripMenuItem1});
            this.notifyIconMenu.Name = "notifyIconMenu";
            // 
            // openToolStripMenuItem
            // 
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem1
            // 
            resources.ApplyResources(this.quitToolStripMenuItem1, "quitToolStripMenuItem1");
            this.quitToolStripMenuItem1.Name = "quitToolStripMenuItem1";
            this.quitToolStripMenuItem1.Click += new System.EventHandler(this.quitToolStripMenuItem1_Click);
            // 
            // btnRescan
            // 
            resources.ApplyResources(this.btnRescan, "btnRescan");
            this.btnRescan.Name = "btnRescan";
            this.btnRescan.UseVisualStyleBackColor = true;
            this.btnRescan.Click += new System.EventHandler(this.RescanToolStripMenuItem_Click);
            // 
            // lblNoDevicesFound
            // 
            resources.ApplyResources(this.lblNoDevicesFound, "lblNoDevicesFound");
            this.lblNoDevicesFound.BackColor = System.Drawing.Color.Transparent;
            this.lblNoDevicesFound.Name = "lblNoDevicesFound";
            // 
            // lblInitializing
            // 
            resources.ApplyResources(this.lblInitializing, "lblInitializing");
            this.lblInitializing.BackColor = System.Drawing.Color.Transparent;
            this.lblInitializing.Name = "lblInitializing";
            // 
            // btnConnectManually
            // 
            resources.ApplyResources(this.btnConnectManually, "btnConnectManually");
            this.btnConnectManually.Name = "btnConnectManually";
            this.btnConnectManually.UseVisualStyleBackColor = true;
            this.btnConnectManually.Click += new System.EventHandler(this.ManualConnection_Click);
            // 
            // btnCancelShare
            // 
            resources.ApplyResources(this.btnCancelShare, "btnCancelShare");
            this.btnCancelShare.Name = "btnCancelShare";
            this.btnCancelShare.UseVisualStyleBackColor = true;
            this.btnCancelShare.Click += new System.EventHandler(this.btnCancelShare_Click);
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCancelShare);
            this.Controls.Add(this.btnConnectManually);
            this.Controls.Add(this.lblInitializing);
            this.Controls.Add(this.lblNoDevicesFound);
            this.Controls.Add(this.btnRescan);
            this.Controls.Add(this.flowLayoutPanel);
            this.Controls.Add(this.lblDevices);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.notifyIconMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblIP;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rescanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectionIssuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gitHubToolStripMenuItem;
        private System.Windows.Forms.Label lblDevices;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.ToolStripMenuItem reannounceToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip notifyIconMenu;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem1;
        private System.Windows.Forms.Button btnRescan;
        private System.Windows.Forms.Label lblNoDevicesFound;
        private System.Windows.Forms.Label lblInitializing;
        private System.Windows.Forms.ToolStripMenuItem manualConnectionToolStripMenuItem;
        private System.Windows.Forms.Button btnConnectManually;
        private System.Windows.Forms.Button btnCancelShare;
    }
}

