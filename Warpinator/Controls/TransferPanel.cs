using System;
using System.IO;
using System.Windows.Forms;

namespace Warpinator.Controls
{
    public partial class TransferPanel : UserControl
    {
        private readonly Transfer transfer;

        public TransferPanel(Transfer t)
        {
            InitializeComponent();

            transfer = t;
            if (t.FileCount == 1)
                imgFile.Image = Utils.GetFileIcon(t.SingleName, true).ToBitmap();
            else if (t.Message != null)
                imgFile.Image = t.Direction == TransferDirection.SEND ? Properties.Resources.message_sent : Properties.Resources.message_received;
            else
                imgFile.Image = Properties.Resources.files;
            UpdateControls();
            transfer.TransferUpdated += OnTransferUpdated;
            Disposed += OnDisposed;
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            Disposed -= OnDisposed;
            transfer.TransferUpdated -= OnTransferUpdated;
        }

        private void OnTransferUpdated(object s, EventArgs a)
        {
            try
            {
                Invoke(new Action(() => UpdateControls()));
            }
            catch (ObjectDisposedException) { } //Sometimes happens
        }

        private void UpdateControls()
        {
            bool isTextMessage = transfer.Message != null;
            btnAccept.Visible = (transfer.Status == TransferStatus.WAITING_PERMISSION) && (transfer.Direction == TransferDirection.RECEIVE);
            btnDecline.Visible = transfer.Status == TransferStatus.WAITING_PERMISSION;
            btnStop.Visible = transfer.Status == TransferStatus.TRANSFERRING;
            progressBar.Visible = transfer.Status == TransferStatus.TRANSFERRING;
            btnRestart.Visible = (transfer.Direction == TransferDirection.SEND) &&
                (transfer.Status == TransferStatus.FAILED || transfer.Status == TransferStatus.STOPPED);
            btnCopy.Visible = isTextMessage && (transfer.Status == TransferStatus.FINISHED && transfer.Direction == TransferDirection.RECEIVE);

            txtTransfer.Text = transfer.Message?.Replace("\n", "\r\n") ?? (transfer.FileCount == 1 ? transfer.SingleName : String.Format(Resources.Strings.files, transfer.FileCount)) + " (" + Utils.BytesToHumanReadable((long)transfer.TotalSize) + ")";
            txtTransfer.Multiline = isTextMessage && transfer.Status == TransferStatus.FINISHED;
            txtTransfer.Enabled = isTextMessage; //Disable scrolling for non-message transfer
            txtTransfer.Select(0, 0); //Unselect autoselected text
            // Status label
            if (transfer.Status == TransferStatus.WAITING_PERMISSION)
            {
                lblProgress.Text = Resources.Strings.waiting_for_permission;
                if (transfer.OverwriteWarning)
                    lblProgress.Text += Resources.Strings.files_may_be_overwritten;
            }
            else if (transfer.Status == TransferStatus.TRANSFERRING)
                lblProgress.Text = Utils.BytesToHumanReadable(transfer.BytesTransferred) + " / " + Utils.BytesToHumanReadable((long)transfer.TotalSize) + " (" +
                    Utils.BytesToHumanReadable(transfer.BytesPerSecond.GetMovingAverage()) + "/s, " + String.Format(Resources.Strings.remaining, transfer.GetRemainingTime()) + ")";
            else
                lblProgress.Text = (isTextMessage && transfer.Status == TransferStatus.FINISHED) ? "" :  transfer.GetStatusString();
            progressBar.Value = (int)(transfer.Progress * 100);
            btnShowDetails.Visible = (transfer.Status == TransferStatus.FINISHED_WITH_ERRORS) || (transfer.Status == TransferStatus.FAILED && transfer.errors.Count > 0);
        }

        private void BtnAccept_Click(object sender, EventArgs e)
        {
            // Create download dir if it doesn't exist
            if (!Directory.Exists(Properties.Settings.Default.DownloadDir))
            {
                try {
                    Directory.CreateDirectory(Properties.Settings.Default.DownloadDir);
                } catch {
                    MessageBox.Show(Resources.Strings.cannot_create_dldir, Resources.Strings.error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            transfer.StartReceiving();
        }

        private void BtnDecline_Click(object sender, EventArgs e)
        {
            transfer.DeclineTransfer();
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            transfer.errors.Clear();
            if (transfer.Message != null)
            {
                transfer.Status = TransferStatus.TRANSFERRING;
                Server.current.Remotes[transfer.RemoteUUID].SendTextMessage(transfer);
            }
            else
            {
                transfer.Status = TransferStatus.WAITING_PERMISSION;
                Server.current.Remotes[transfer.RemoteUUID].StartSendTransfer(transfer);
            }
            UpdateControls();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            transfer.Stop();
        }

        private void BtnShowDetails_Click(object sender, EventArgs e)
        {            
            MessageBox.Show(ParentForm, String.Join("\n", transfer.errors), Resources.Strings.transfer);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(transfer.Message);
        }
    }
}
