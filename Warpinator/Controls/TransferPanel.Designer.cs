
namespace Warpinator.Controls
{
    partial class TransferPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransferPanel));
            this.lblFiles = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnAccept = new System.Windows.Forms.Button();
            this.btnDecline = new System.Windows.Forms.Button();
            this.btnRestart = new System.Windows.Forms.Button();
            this.btnShowDetails = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.imgFile = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.imgFile)).BeginInit();
            this.SuspendLayout();
            // 
            // lblFiles
            // 
            resources.ApplyResources(this.lblFiles, "lblFiles");
            this.lblFiles.Name = "lblFiles";
            // 
            // lblProgress
            // 
            resources.ApplyResources(this.lblProgress, "lblProgress");
            this.lblProgress.Name = "lblProgress";
            // 
            // progressBar
            // 
            resources.ApplyResources(this.progressBar, "progressBar");
            this.progressBar.Name = "progressBar";
            // 
            // btnAccept
            // 
            resources.ApplyResources(this.btnAccept, "btnAccept");
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.UseVisualStyleBackColor = true;
            this.btnAccept.Click += new System.EventHandler(this.BtnAccept_Click);
            // 
            // btnDecline
            // 
            resources.ApplyResources(this.btnDecline, "btnDecline");
            this.btnDecline.Name = "btnDecline";
            this.btnDecline.UseVisualStyleBackColor = true;
            this.btnDecline.Click += new System.EventHandler(this.BtnDecline_Click);
            // 
            // btnRestart
            // 
            resources.ApplyResources(this.btnRestart, "btnRestart");
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.BtnRestart_Click);
            // 
            // btnShowDetails
            // 
            resources.ApplyResources(this.btnShowDetails, "btnShowDetails");
            this.btnShowDetails.Name = "btnShowDetails";
            this.btnShowDetails.UseVisualStyleBackColor = true;
            this.btnShowDetails.Click += new System.EventHandler(this.BtnShowDetails_Click);
            // 
            // btnStop
            // 
            resources.ApplyResources(this.btnStop, "btnStop");
            this.btnStop.BackgroundImage = global::Warpinator.Properties.Resources.stop;
            this.btnStop.Name = "btnStop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // 
            // imgFile
            // 
            resources.ApplyResources(this.imgFile, "imgFile");
            this.imgFile.Name = "imgFile";
            this.imgFile.TabStop = false;
            // 
            // TransferPanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnShowDetails);
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.btnDecline);
            this.Controls.Add(this.btnAccept);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.lblFiles);
            this.Controls.Add(this.imgFile);
            this.Name = "TransferPanel";
            ((System.ComponentModel.ISupportInitialize)(this.imgFile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox imgFile;
        private System.Windows.Forms.Label lblFiles;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnAccept;
        private System.Windows.Forms.Button btnDecline;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Button btnShowDetails;
    }
}
