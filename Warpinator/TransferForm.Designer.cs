
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
            this.pictureUser = new System.Windows.Forms.PictureBox();
            this.lblDisplayName = new System.Windows.Forms.Label();
            this.lblUserString = new System.Windows.Forms.Label();
            this.lblAddress = new System.Windows.Forms.Label();
            this.pictureStatus = new System.Windows.Forms.PictureBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.flowLayoutTransfers = new System.Windows.Forms.FlowLayoutPanel();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.btnDlDir = new System.Windows.Forms.Button();
            this.browseFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseFoldersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnBrowseDir = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureUser
            // 
            this.pictureUser.Location = new System.Drawing.Point(12, 12);
            this.pictureUser.Name = "pictureUser";
            this.pictureUser.Size = new System.Drawing.Size(64, 64);
            this.pictureUser.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureUser.TabIndex = 0;
            this.pictureUser.TabStop = false;
            // 
            // lblDisplayName
            // 
            this.lblDisplayName.AutoSize = true;
            this.lblDisplayName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblDisplayName.Location = new System.Drawing.Point(82, 12);
            this.lblDisplayName.Name = "lblDisplayName";
            this.lblDisplayName.Size = new System.Drawing.Size(38, 17);
            this.lblDisplayName.TabIndex = 1;
            this.lblDisplayName.Text = "User";
            // 
            // lblUserString
            // 
            this.lblUserString.AutoSize = true;
            this.lblUserString.Location = new System.Drawing.Point(82, 35);
            this.lblUserString.Name = "lblUserString";
            this.lblUserString.Size = new System.Drawing.Size(104, 13);
            this.lblUserString.TabIndex = 2;
            this.lblUserString.Text = "username@machine";
            // 
            // lblAddress
            // 
            this.lblAddress.AutoSize = true;
            this.lblAddress.Location = new System.Drawing.Point(82, 58);
            this.lblAddress.Name = "lblAddress";
            this.lblAddress.Size = new System.Drawing.Size(97, 13);
            this.lblAddress.TabIndex = 3;
            this.lblAddress.Text = "192.168.1.1:42000";
            // 
            // pictureStatus
            // 
            this.pictureStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureStatus.Location = new System.Drawing.Point(360, 12);
            this.pictureStatus.Name = "pictureStatus";
            this.pictureStatus.Size = new System.Drawing.Size(32, 32);
            this.pictureStatus.TabIndex = 4;
            this.pictureStatus.TabStop = false;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(317, 12);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Status";
            // 
            // flowLayoutTransfers
            // 
            this.flowLayoutTransfers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutTransfers.AutoScroll = true;
            this.flowLayoutTransfers.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutTransfers.Location = new System.Drawing.Point(12, 82);
            this.flowLayoutTransfers.Name = "flowLayoutTransfers";
            this.flowLayoutTransfers.Size = new System.Drawing.Size(380, 179);
            this.flowLayoutTransfers.TabIndex = 6;
            this.flowLayoutTransfers.WrapContents = false;
            this.flowLayoutTransfers.ClientSizeChanged += new System.EventHandler(this.FlowLayoutPanel_ClientSizeChanged);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBrowse.Location = new System.Drawing.Point(12, 267);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 7;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(319, 267);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 8;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.BtnSend_Click);
            // 
            // txtFile
            // 
            this.txtFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFile.Location = new System.Drawing.Point(110, 269);
            this.txtFile.Name = "txtFile";
            this.txtFile.ReadOnly = true;
            this.txtFile.Size = new System.Drawing.Size(203, 20);
            this.txtFile.TabIndex = 9;
            // 
            // btnDlDir
            // 
            this.btnDlDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDlDir.Location = new System.Drawing.Point(258, 53);
            this.btnDlDir.Name = "btnDlDir";
            this.btnDlDir.Size = new System.Drawing.Size(134, 23);
            this.btnDlDir.TabIndex = 10;
            this.btnDlDir.Text = "Open download folder";
            this.btnDlDir.UseVisualStyleBackColor = true;
            // 
            // browseFilesToolStripMenuItem
            // 
            this.browseFilesToolStripMenuItem.Name = "browseFilesToolStripMenuItem";
            this.browseFilesToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.browseFilesToolStripMenuItem.Text = "Browse files";
            // 
            // browseFoldersToolStripMenuItem
            // 
            this.browseFoldersToolStripMenuItem.Name = "browseFoldersToolStripMenuItem";
            this.browseFoldersToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.browseFoldersToolStripMenuItem.Text = "Browse folders";
            // 
            // btnBrowseDir
            // 
            this.btnBrowseDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBrowseDir.Location = new System.Drawing.Point(85, 267);
            this.btnBrowseDir.Name = "btnBrowseDir";
            this.btnBrowseDir.Size = new System.Drawing.Size(19, 23);
            this.btnBrowseDir.TabIndex = 11;
            this.btnBrowseDir.Text = "+";
            this.btnBrowseDir.UseVisualStyleBackColor = true;
            this.btnBrowseDir.Click += new System.EventHandler(this.BtnBrowseDir_Click);
            // 
            // TransferForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 302);
            this.Controls.Add(this.btnBrowseDir);
            this.Controls.Add(this.btnDlDir);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.flowLayoutTransfers);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pictureStatus);
            this.Controls.Add(this.lblAddress);
            this.Controls.Add(this.lblUserString);
            this.Controls.Add(this.lblDisplayName);
            this.Controls.Add(this.pictureUser);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(360, 220);
            this.Name = "TransferForm";
            this.Text = "TransferForm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureUser;
        private System.Windows.Forms.Label lblDisplayName;
        private System.Windows.Forms.Label lblUserString;
        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.PictureBox pictureStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutTransfers;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Button btnDlDir;
        private System.Windows.Forms.ToolStripMenuItem browseFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browseFoldersToolStripMenuItem;
        private System.Windows.Forms.Button btnBrowseDir;
    }
}