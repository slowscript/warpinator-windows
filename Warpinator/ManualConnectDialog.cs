using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Warpinator
{
    public partial class ManualConnectDialog : Form
    {
        public ManualConnectDialog()
        {
            InitializeComponent();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            string myAddress = $"{Server.current.SelectedIP}:{Server.current.AuthPort}";
            lblIP.Text = myAddress;

            var qrcoder = new QRCoder.QRCodeGenerator();
            var qr = qrcoder.CreateQrCode($"warpinator://{myAddress}", QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrBitmap = new QRCoder.BitmapByteQRCode(qr);
            var qrBytes = qrBitmap.GetGraphic(pixelsPerModule: 5, ColorToRGB(Color.Black), ColorToRGB(SystemColors.Control));
            imgQRCode.Image = Image.FromStream(new MemoryStream(qrBytes));
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            btnConnect.Enabled = false;
            string result = await Server.current.RegisterWithHost(txtIP.Text);
            btnConnect.Enabled = true;
            Cursor = Cursors.Default;
            if (result == null)
                Close();
            else
                MessageBox.Show(result, Resources.Strings.manual_connection_failed, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private byte[] ColorToRGB(Color c)
        {
            return new byte[] { c.R, c.G, c.B };
        }

        private void txtIP_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnConnect.PerformClick();
                e.Handled = true; //suppress the beep
            }
        }
    }
}
