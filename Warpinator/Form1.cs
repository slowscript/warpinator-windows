using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Warpinator.Controls;

namespace Warpinator
{
    public partial class Form1 : Form
    {
        readonly Server server;
        static Form1 current;

        public Form1()
        {
            current = this;
            InitializeComponent();
            server = new Server();
            flowLayoutPanel.ClientSizeChanged += FlowLayoutPanel_ClientSizeChanged;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //server.Remotes.Add("a", new Remote { DisplayName = "TEST", UserName = "test", Hostname = "PC1", Address = System.Net.IPAddress.Parse("192.168.1.1"), Port = 42000 });

            server.Start();
        }

        private async void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            current = null;
            await server.Stop();
        }

        public static void UpdateUI()
        {
            if (current != null)
                current.Invoke(new Action(() => current.DoUpdateUI()));
        }

        private void DoUpdateUI()
        {
            flowLayoutPanel.Controls.Clear();
            foreach (var r in server.Remotes.Values)
            {
                var btn = new RemoteButton(r);
                btn.UpdateInfo();
                flowLayoutPanel.Controls.Add(btn);
                btn.Width = flowLayoutPanel.ClientSize.Width - 10;
                btn.Show();
            }
            
            string iface = Makaretu.Dns.MulticastService.GetNetworkInterfaces().FirstOrDefault((i) => i.Id == server.SelectedInterface)?.Name ?? "Selected interface unavailable";
            if (String.IsNullOrEmpty(server.SelectedInterface))
                iface = "Any";
            lblIP.Text = Utils.GetLocalIPAddress() + " | " + iface;
            
            lblStatus.Text = server.Running ? "Service is running" : "Service not running!";
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        private void FlowLayoutPanel_ClientSizeChanged(object sender, EventArgs e)
        {
            foreach(Control c in flowLayoutPanel.Controls)
            {
                c.Width = flowLayoutPanel.ClientSize.Width - 10;
            }
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SettingsForm().Show();
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e) => Close();
        private void RescanToolStripMenuItem_Click(object sender, EventArgs e) => server.Rescan();
        private void ReannounceToolStripMenuItem_Click(object sender, EventArgs e) => server.Reannounce();
    }
}
