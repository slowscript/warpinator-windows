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
            btnAccept.Visible = (transfer.Status == Transfer.TransferStatus.WAITING_PERMISSION) && (transfer.Direction == Transfer.TransferDirection.RECEIVE);
            btnDecline.Visible = transfer.Status == Transfer.TransferStatus.WAITING_PERMISSION;
            btnStop.Visible = transfer.Status == Transfer.TransferStatus.TRANSFERRING;
            progressBar.Visible = transfer.Status == Transfer.TransferStatus.TRANSFERRING;
            btnRestart.Visible = (transfer.Direction == Transfer.TransferDirection.SEND) &&
                (transfer.Status == Transfer.TransferStatus.FAILED || transfer.Status == Transfer.TransferStatus.STOPPED);
            
            lblFiles.Text = (transfer.FileCount == 1 ? transfer.SingleName : transfer.FileCount + " files") + " (" + Utils.BytesToHumanReadable((long)transfer.TotalSize) + ")";
            // Status label
            if (transfer.Status == Transfer.TransferStatus.WAITING_PERMISSION)
            {
                lblProgress.Text = "Waiting for permission";
                if (transfer.OverwriteWarning)
                    lblProgress.Text += " (Files may be overwritten!)";
            }
            else if (transfer.Status == Transfer.TransferStatus.TRANSFERRING)
                lblProgress.Text = Utils.BytesToHumanReadable(transfer.BytesTransferred) + " / " + Utils.BytesToHumanReadable((long)transfer.TotalSize) + " (" +
                Utils.BytesToHumanReadable(transfer.BytesPerSecond) + "/s, " + transfer.GetRemainingTime() + " remaining)";
            else
                lblProgress.Text = transfer.Status.ToString();
            progressBar.Value = (int)(transfer.Progress * 100);
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
            transfer.Status = Transfer.TransferStatus.WAITING_PERMISSION;
            UpdateControls();
            Server.current.Remotes[transfer.RemoteUUID].StartSendTransfer(transfer);
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            transfer.Stop();
        }
    }
}
