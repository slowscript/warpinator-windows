
namespace Warpinator.Controls
{
    partial class RemoteButton
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

        #region Kód vygenerovaný pomocí Návrháře komponent

        /// <summary> 
        /// Metoda vyžadovaná pro podporu Návrháře - neupravovat
        /// obsah této metody v editoru kódu.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSelf = new System.Windows.Forms.Button();
            this.lblDisplayName = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.lblIP = new System.Windows.Forms.Label();
            this.pictureBoxStatus = new System.Windows.Forms.PictureBox();
            this.pictureBoxProfile = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelf
            // 
            this.btnSelf.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelf.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelf.Location = new System.Drawing.Point(3, 3);
            this.btnSelf.Name = "btnSelf";
            this.btnSelf.Size = new System.Drawing.Size(344, 64);
            this.btnSelf.TabIndex = 0;
            this.btnSelf.UseVisualStyleBackColor = true;
            this.btnSelf.Click += new System.EventHandler(this.BtnSelf_Click);
            // 
            // lblDisplayName
            // 
            this.lblDisplayName.AutoSize = true;
            this.lblDisplayName.BackColor = System.Drawing.Color.Transparent;
            this.lblDisplayName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblDisplayName.Location = new System.Drawing.Point(68, 8);
            this.lblDisplayName.Name = "lblDisplayName";
            this.lblDisplayName.Size = new System.Drawing.Size(91, 17);
            this.lblDisplayName.TabIndex = 2;
            this.lblDisplayName.Text = "DisplayName";
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.BackColor = System.Drawing.Color.Transparent;
            this.lblUser.Location = new System.Drawing.Point(68, 29);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(107, 13);
            this.lblUser.TabIndex = 3;
            this.lblUser.Text = "userName@Machine";
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(68, 49);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(97, 13);
            this.lblIP.TabIndex = 5;
            this.lblIP.Text = "192.168.1.1:42000";
            // 
            // pictureBoxStatus
            // 
            this.pictureBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxStatus.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxStatus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBoxStatus.Location = new System.Drawing.Point(301, 20);
            this.pictureBoxStatus.Name = "pictureBoxStatus";
            this.pictureBoxStatus.Size = new System.Drawing.Size(36, 33);
            this.pictureBoxStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxStatus.TabIndex = 4;
            this.pictureBoxStatus.TabStop = false;
            // 
            // pictureBoxProfile
            // 
            this.pictureBoxProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBoxProfile.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxProfile.Location = new System.Drawing.Point(8, 8);
            this.pictureBoxProfile.Name = "pictureBoxProfile";
            this.pictureBoxProfile.Size = new System.Drawing.Size(54, 54);
            this.pictureBoxProfile.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxProfile.TabIndex = 1;
            this.pictureBoxProfile.TabStop = false;
            // 
            // RemoteButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.Controls.Add(this.lblIP);
            this.Controls.Add(this.pictureBoxStatus);
            this.Controls.Add(this.lblUser);
            this.Controls.Add(this.lblDisplayName);
            this.Controls.Add(this.pictureBoxProfile);
            this.Controls.Add(this.btnSelf);
            this.MaximumSize = new System.Drawing.Size(500, 70);
            this.Name = "RemoteButton";
            this.Size = new System.Drawing.Size(350, 70);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxProfile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSelf;
        private System.Windows.Forms.PictureBox pictureBoxProfile;
        private System.Windows.Forms.Label lblDisplayName;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.PictureBox pictureBoxStatus;
        private System.Windows.Forms.Label lblIP;
    }
}
