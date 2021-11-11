using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Warpinator
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.warplogo;
            LoadSettings();
        }

        private void LoadSettings()
        {
            txtRecvDir.Text = Properties.Settings.Default.DownloadDir;
        }

        private void ApplyNetwork()
        {
        
        }

        private void Apply()
        {
            ApplyNetwork();
            if (System.IO.Directory.Exists(txtRecvDir.Text))
                Properties.Settings.Default.DownloadDir = txtRecvDir.Text;
            else
                MessageBox.Show("Selected directory does not exist, therefore this setting will not change", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            var d = new OpenFoldersDialog();
            if (d.Show())
                txtRecvDir.Text = d.FileNames[0];
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {

        }

        private void BtnCancel_Click(object sender, EventArgs e) => Close();
        private void BtnApply_Click(object sender, EventArgs e) => Apply();
        private void BtnOK_Click(object sender, EventArgs e)
        {
            Apply();
            Close();
        }

    }
}
