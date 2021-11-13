using System;
using System.Drawing;
using System.Windows.Forms;

namespace Warpinator.Controls
{
    public partial class RemoteButton : UserControl
    {
        public string DisplayName { get { return lblDisplayName.Text; } set { lblDisplayName.Text = value; } }
        public string UserString { get { return lblUser.Text; } set { lblUser.Text = value; } }
        public string IPString { get { return lblIP.Text; } set { lblIP.Text = value; } }
        public Image ProfilePicture { set { pictureBoxProfile.Image = value; } }
        public Remote.RemoteStatus Status { set {
                switch (value)
                {
                    case Remote.RemoteStatus.AWAITING_DUPLEX:
                        pictureBoxStatus.Image = Properties.Resources.awaiting_duplex; break;
                    case Remote.RemoteStatus.CONNECTING:
                        pictureBoxStatus.Image = Properties.Resources.connecting; break;
                    case Remote.RemoteStatus.CONNECTED:
                        pictureBoxStatus.Image = Properties.Resources.connected; break;
                    case Remote.RemoteStatus.DISCONNECTED:
                        pictureBoxStatus.Image = Properties.Resources.disconnected; break;
                    case Remote.RemoteStatus.ERROR:
                        pictureBoxStatus.Image = Properties.Resources.error; break;
                }
            }
        }

        private readonly Remote remote;

        public RemoteButton(Remote r)
        {
            InitializeComponent();
            
            remote = r;
            UpdateInfo();
        }

        public void UpdateInfo()
        {
            DisplayName = remote.DisplayName;
            UserString = remote.UserName + "@" + remote.Hostname;
            IPString = remote.Address + ":" + remote.Port;
            ProfilePicture = remote.Picture;
            Status = remote.Status;
        }

        private void BtnSelf_Click(object sender, EventArgs e)
        {
            remote.OpenWindow();
        }
    }
}
