using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Makaretu.Dns;

namespace Warpinator
{
    public partial class SettingsForm : Form
    {
        Dictionary<string, string> ifaceDict = new Dictionary<string, string>();
        public SettingsForm()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.warplogo;
            LoadSettings();
        }

        private void LoadSettings()
        {
            txtRecvDir.Text = Properties.Settings.Default.DownloadDir;
            txtGroupcode.Text = Properties.Settings.Default.GroupCode;
            numPort.Value = Properties.Settings.Default.Port;

            var ifaces = MulticastService.GetNetworkInterfaces();
            ifaceDict.Clear();
            ifaceDict.Add("", "Any");
            string selecetedIfaceId = Properties.Settings.Default.NetworkInterface;
            int id = -1;
            int i = 1;
            foreach (var iface in ifaces)
            {
                ifaceDict.Add(iface.Id, iface.Name);
                if (iface.Id == selecetedIfaceId)
                    id = i;
                i++;
            }
            if (selecetedIfaceId == "")
                id = 0;
            else if (id == -1)
            {
                ifaceDict.Add(selecetedIfaceId, $"Unavailable interface: \"{selecetedIfaceId}\"");
                id = i;
            }
            comboInterfaces.Items.AddRange(ifaceDict.Values.ToArray());
            comboInterfaces.SelectedIndex = id;
        }

        private void ApplyNetwork()
        {
            Properties.Settings.Default.NetworkInterface = ifaceDict.Keys.ToArray()[comboInterfaces.SelectedIndex];
            Properties.Settings.Default.Port = (int)numPort.Value;
            Properties.Settings.Default.GroupCode = txtGroupcode.Text;
        }

        private void Apply()
        {
            ApplyNetwork();
            if (System.IO.Directory.Exists(txtRecvDir.Text))
                Properties.Settings.Default.DownloadDir = txtRecvDir.Text;
            else
                MessageBox.Show("Selected directory does not exist, therefore this setting will not change", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
            Properties.Settings.Default.Save();
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            var d = new OpenFoldersDialog();
            if (d.Show())
                txtRecvDir.Text = d.FileNames[0];
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            ApplyNetwork();
            Server.current.Restart();
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
