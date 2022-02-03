using System;
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
            Invoke(new Action(() => UpdateControls()));
        }

        private void UpdateControls()
        {
            btnAccept.Visible = (transfer.Status == TransferStatus.WAITING_PERMISSION) && (transfer.Direction == TransferDirection.RECEIVE);
            btnDecline.Visible = transfer.Status == TransferStatus.WAITING_PERMISSION;
            btnStop.Visible = transfer.Status == TransferStatus.TRANSFERRING;
            progressBar.Visible = transfer.Status == TransferStatus.TRANSFERRING;
            btnRestart.Visible = (transfer.Direction == TransferDirection.SEND) &&
                (transfer.Status == TransferStatus.FAILED || transfer.Status == TransferStatus.STOPPED);
            
            lblFiles.Text = (transfer.FileCount == 1 ? transfer.SingleName : String.Format(Resources.Strings.files, transfer.FileCount)) + " (" + Utils.BytesToHumanReadable((long)transfer.TotalSize) + ")";
            // Status label
            if (transfer.Status == TransferStatus.WAITING_PERMISSION)
            {
                lblProgress.Text = Resources.Strings.waiting_for_permission;
                if (transfer.OverwriteWarning)
                    lblProgress.Text += Resources.Strings.files_may_be_overwritten;
            }
            else if (transfer.Status == TransferStatus.TRANSFERRING)
                lblProgress.Text = Utils.BytesToHumanReadable(transfer.BytesTransferred) + " / " + Utils.BytesToHumanReadable((long)transfer.TotalSize) + " (" +
                Utils.BytesToHumanReadable(transfer.BytesPerSecond) + "/s, " + String.Format(Resources.Strings.remaining, transfer.GetRemainingTime()) + ")";
            else
                lblProgress.Text = transfer.GetStatusString();
            progressBar.Value = (int)(transfer.Progress * 100);
            btnShowDetails.Visible = (transfer.Status == TransferStatus.FINISHED_WITH_ERRORS) || (transfer.Status == TransferStatus.FAILED && transfer.errors.Count > 0);
        }

        private void BtnAccept_Click(object sender, EventArgs e)
        {
            transfer.StartReceiving();
        }

        private void BtnDecline_Click(object sender, EventArgs e)
        {
            transfer.DeclineTransfer();
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            transfer.Status = TransferStatus.WAITING_PERMISSION;
            transfer.errors.Clear();
            UpdateControls();
            Server.current.Remotes[transfer.RemoteUUID].StartSendTransfer(transfer);
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            transfer.Stop();
        }

        private void BtnShowDetails_Click(object sender, EventArgs e)
        {            
            MessageBox.Show(ParentForm, String.Join("\n", transfer.errors), Resources.Strings.transfer);
        }
    }
}
