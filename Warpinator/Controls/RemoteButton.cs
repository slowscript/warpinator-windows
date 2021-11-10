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
        public Remote.RemoteStatus Status { set { /* TODO: Status picture */ } }

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
        }

        private void BtnSelf_Click(object sender, EventArgs e)
        {
            remote.OpenWindow();
        }
    }
}
