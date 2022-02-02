
namespace Warpinator
{
    partial class TransferForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransferForm));
            this.pictureUser = new System.Windows.Forms.PictureBox();
            this.lblDisplayName = new System.Windows.Forms.Label();
            this.lblUserString = new System.Windows.Forms.Label();
            this.lblAddress = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.flowLayoutTransfers = new System.Windows.Forms.FlowLayoutPanel();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.btnDlDir = new System.Windows.Forms.Button();
            this.browseFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseFoldersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnBrowseDir = new System.Windows.Forms.Button();
            this.btnReconnect = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureUser)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureUser
            // 
            resources.ApplyResources(this.pictureUser, "pictureUser");
            this.pictureUser.Name = "pictureUser";
            this.pictureUser.TabStop = false;
            // 
            // lblDisplayName
            // 
            resources.ApplyResources(this.lblDisplayName, "lblDisplayName");
            this.lblDisplayName.Name = "lblDisplayName";
            // 
            // lblUserString
            // 
            resources.ApplyResources(this.lblUserString, "lblUserString");
            this.lblUserString.Name = "lblUserString";
            // 
            // lblAddress
            // 
            resources.ApplyResources(this.lblAddress, "lblAddress");
            this.lblAddress.Name = "lblAddress";
            // 
            // lblStatus
            // 
            resources.ApplyResources(this.lblStatus, "lblStatus");
            this.lblStatus.Name = "lblStatus";
            // 
            // flowLayoutTransfers
            // 
            resources.ApplyResources(this.flowLayoutTransfers, "flowLayoutTransfers");
            this.flowLayoutTransfers.AllowDrop = true;
            this.flowLayoutTransfers.Name = "flowLayoutTransfers";
            this.flowLayoutTransfers.ClientSizeChanged += new System.EventHandler(this.FlowLayoutPanel_ClientSizeChanged);
            this.flowLayoutTransfers.DragDrop += new System.Windows.Forms.DragEventHandler(this.FlowLayoutTransfers_DragDrop);
            this.flowLayoutTransfers.DragEnter += new System.Windows.Forms.DragEventHandler(this.DropTargets_DragEnter);
            // 
            // btnBrowse
            // 
            resources.ApplyResources(this.btnBrowse, "btnBrowse");
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            // 
            // btnSend
            // 
            resources.ApplyResources(this.btnSend, "btnSend");
            this.btnSend.Name = "btnSend";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.BtnSend_Click);
            // 
            // txtFile
            // 
            resources.ApplyResources(this.txtFile, "txtFile");
            this.txtFile.AllowDrop = true;
            this.txtFile.Name = "txtFile";
            this.txtFile.ReadOnly = true;
            this.txtFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.TxtFile_DragDrop);
            this.txtFile.DragEnter += new System.Windows.Forms.DragEventHandler(this.DropTargets_DragEnter);
            // 
            // btnDlDir
            // 
            resources.ApplyResources(this.btnDlDir, "btnDlDir");
            this.btnDlDir.Name = "btnDlDir";
            this.btnDlDir.UseVisualStyleBackColor = true;
            this.btnDlDir.Click += new System.EventHandler(this.BtnDlDir_Click);
            // 
            // browseFilesToolStripMenuItem
            // 
            resources.ApplyResources(this.browseFilesToolStripMenuItem, "browseFilesToolStripMenuItem");
            this.browseFilesToolStripMenuItem.Name = "browseFilesToolStripMenuItem";
            // 
            // browseFoldersToolStripMenuItem
            // 
            resources.ApplyResources(this.browseFoldersToolStripMenuItem, "browseFoldersToolStripMenuItem");
            this.browseFoldersToolStripMenuItem.Name = "browseFoldersToolStripMenuItem";
            // 
            // btnBrowseDir
            // 
            resources.ApplyResources(this.btnBrowseDir, "btnBrowseDir");
            this.btnBrowseDir.Name = "btnBrowseDir";
            this.btnBrowseDir.UseVisualStyleBackColor = true;
            this.btnBrowseDir.Click += new System.EventHandler(this.BtnBrowseDir_Click);
            // 
            // btnReconnect
            // 
            resources.ApplyResources(this.btnReconnect, "btnReconnect");
            this.btnReconnect.Name = "btnReconnect";
            this.btnReconnect.UseVisualStyleBackColor = true;
            this.btnReconnect.Click += new System.EventHandler(this.BtnReconnect_Click);
            // 
            // TransferForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnReconnect);
            this.Controls.Add(this.btnBrowseDir);
            this.Controls.Add(this.btnDlDir);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.flowLayoutTransfers);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblAddress);
            this.Controls.Add(this.lblUserString);
            this.Controls.Add(this.lblDisplayName);
            this.Controls.Add(this.pictureUser);
            this.MaximizeBox = false;
            this.Name = "TransferForm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureUser)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureUser;
        private System.Windows.Forms.Label lblDisplayName;
        private System.Windows.Forms.Label lblUserString;
        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutTransfers;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Button btnDlDir;
        private System.Windows.Forms.ToolStripMenuItem browseFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browseFoldersToolStripMenuItem;
        private System.Windows.Forms.Button btnBrowseDir;
        private System.Windows.Forms.Button btnReconnect;
    }
}