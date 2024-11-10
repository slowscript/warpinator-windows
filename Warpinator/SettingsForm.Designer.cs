
namespace Warpinator
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.label1 = new System.Windows.Forms.Label();
            this.txtRecvDir = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtGroupcode = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numAuthPort = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.btnShowCode = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.comboInterfaces = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnRestart = new System.Windows.Forms.Button();
            this.chkNotify = new System.Windows.Forms.CheckBox();
            this.chkOverwrite = new System.Windows.Forms.CheckBox();
            this.chkAutoAccept = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.chkBackground = new System.Windows.Forms.CheckBox();
            this.chkUpdates = new System.Windows.Forms.CheckBox();
            this.chkStartMinimized = new System.Windows.Forms.CheckBox();
            this.chkCompression = new System.Windows.Forms.CheckBox();
            this.chkRunOnStartup = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAuthPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtRecvDir
            // 
            resources.ApplyResources(this.txtRecvDir, "txtRecvDir");
            this.txtRecvDir.Name = "txtRecvDir";
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.BtnBrowse_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // txtGroupcode
            // 
            resources.ApplyResources(this.txtGroupcode, "txtGroupcode");
            this.txtGroupcode.Name = "txtGroupcode";
            this.txtGroupcode.UseSystemPasswordChar = true;
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.numAuthPort);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.btnShowCode);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.numPort);
            this.groupBox1.Controls.Add(this.comboInterfaces);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnRestart);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtGroupcode);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // numAuthPort
            // 
            resources.ApplyResources(this.numAuthPort, "numAuthPort");
            this.numAuthPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numAuthPort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numAuthPort.Name = "numAuthPort";
            this.numAuthPort.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // btnShowCode
            // 
            resources.ApplyResources(this.btnShowCode, "btnShowCode");
            this.btnShowCode.BackgroundImage = global::Warpinator.Properties.Resources.visible;
            this.btnShowCode.Name = "btnShowCode";
            this.btnShowCode.UseVisualStyleBackColor = true;
            this.btnShowCode.Click += new System.EventHandler(this.BtnShowCode_Click);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // numPort
            // 
            resources.ApplyResources(this.numPort, "numPort");
            this.numPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // comboInterfaces
            // 
            resources.ApplyResources(this.comboInterfaces, "comboInterfaces");
            this.comboInterfaces.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboInterfaces.FormattingEnabled = true;
            this.comboInterfaces.Name = "comboInterfaces";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // btnRestart
            // 
            resources.ApplyResources(this.btnRestart, "btnRestart");
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.BtnRestart_Click);
            // 
            // chkNotify
            // 
            resources.ApplyResources(this.chkNotify, "chkNotify");
            this.chkNotify.Name = "chkNotify";
            this.chkNotify.UseVisualStyleBackColor = true;
            // 
            // chkOverwrite
            // 
            resources.ApplyResources(this.chkOverwrite, "chkOverwrite");
            this.chkOverwrite.Name = "chkOverwrite";
            this.chkOverwrite.UseVisualStyleBackColor = true;
            // 
            // chkAutoAccept
            // 
            resources.ApplyResources(this.chkAutoAccept, "chkAutoAccept");
            this.chkAutoAccept.Name = "chkAutoAccept";
            this.chkAutoAccept.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // btnApply
            // 
            resources.ApplyResources(this.btnApply, "btnApply");
            this.btnApply.Name = "btnApply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.BtnApply_Click);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // chkBackground
            // 
            resources.ApplyResources(this.chkBackground, "chkBackground");
            this.chkBackground.Name = "chkBackground";
            this.chkBackground.UseVisualStyleBackColor = true;
            this.chkBackground.CheckedChanged += new System.EventHandler(this.chkBackground_CheckedChanged);
            // 
            // chkUpdates
            // 
            resources.ApplyResources(this.chkUpdates, "chkUpdates");
            this.chkUpdates.Name = "chkUpdates";
            this.chkUpdates.UseVisualStyleBackColor = true;
            // 
            // chkStartMinimized
            // 
            resources.ApplyResources(this.chkStartMinimized, "chkStartMinimized");
            this.chkStartMinimized.Name = "chkStartMinimized";
            this.chkStartMinimized.UseVisualStyleBackColor = true;
            // 
            // chkCompression
            // 
            resources.ApplyResources(this.chkCompression, "chkCompression");
            this.chkCompression.Name = "chkCompression";
            this.chkCompression.UseVisualStyleBackColor = true;
            // 
            // chkRunOnStartup
            // 
            resources.ApplyResources(this.chkRunOnStartup, "chkRunOnStartup");
            this.chkRunOnStartup.Name = "chkRunOnStartup";
            this.chkRunOnStartup.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkRunOnStartup);
            this.Controls.Add(this.chkCompression);
            this.Controls.Add(this.chkStartMinimized);
            this.Controls.Add(this.chkUpdates);
            this.Controls.Add(this.chkBackground);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.chkAutoAccept);
            this.Controls.Add(this.chkOverwrite);
            this.Controls.Add(this.chkNotify);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtRecvDir);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "SettingsForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAuthPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRecvDir;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtGroupcode;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.ComboBox comboInterfaces;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.CheckBox chkNotify;
        private System.Windows.Forms.CheckBox chkOverwrite;
        private System.Windows.Forms.CheckBox chkAutoAccept;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox chkBackground;
        private System.Windows.Forms.Button btnShowCode;
        private System.Windows.Forms.CheckBox chkUpdates;
        private System.Windows.Forms.CheckBox chkStartMinimized;
        private System.Windows.Forms.NumericUpDown numAuthPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkCompression;
        private System.Windows.Forms.CheckBox chkRunOnStartup;
    }
}