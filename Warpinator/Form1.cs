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
            server.Remotes.Add("a", new Remote { DisplayName = "AAAAAA", UserName = "aaaaaa", Hostname = "PC1", Address = System.Net.IPAddress.Parse("192.168.1.1"), Port = 42000 });
            server.Remotes.Add("b", new Remote { DisplayName = "BBBBBB", UserName = "bbbbbb", Hostname = "PC2", Address = System.Net.IPAddress.Parse("192.168.1.2"), Port = 42000 });
            //server.Remotes.Add("c", new Remote { DisplayName = "CCCCCC", UserName = "cccccc", Hostname = "PC3", Address = System.Net.IPAddress.Parse("192.168.1.3"), Port = 42000 });
            //server.Remotes.Add("d", new Remote { DisplayName = "DDDDDD", UserName = "dddddd", Hostname = "PC4", Address = System.Net.IPAddress.Parse("192.168.1.4"), Port = 42000 });
            //server.Remotes.Add("e", new Remote { DisplayName = "EEEEEE", UserName = "eeeeee", Hostname = "PC5", Address = System.Net.IPAddress.Parse("192.168.1.5"), Port = 42000 });
            //server.Remotes.Add("f", new Remote { DisplayName = "FFFFFF", UserName = "ffffff", Hostname = "PC6", Address = System.Net.IPAddress.Parse("192.168.1.6"), Port = 42000 });
            DoUpdateUI();
            server.Start();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            current = null;
            server.Stop();
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
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().Show();
        }

        private void FlowLayoutPanel_ClientSizeChanged(object sender, EventArgs e)
        {
            foreach(Control c in flowLayoutPanel.Controls)
            {
                c.Width = flowLayoutPanel.ClientSize.Width - 10;
            }
        }
    }
}
