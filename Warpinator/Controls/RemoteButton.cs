﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace Warpinator.Controls
{
    public class RemoteButton : UserControl
    {
        private readonly Remote remote;
        private bool mouseHover = false;
        private bool mouseDown = false;
        public SizeF scale = new SizeF(1f, 1f);

        public RemoteButton(Remote r)
        {
            this.Height = 64;
            this.MaximumSize = new Size(450, 64);
            this.Padding = new Padding(8);
            this.Paint += DrawButton;
            this.DoubleBuffered = true;
            this.MouseEnter += RemoteButton_MouseEnter;
            this.MouseLeave += RemoteButton_MouseLeave;
            this.MouseDown += RemoteButton_MouseDown;
            this.MouseUp += RemoteButton_MouseUp;
            this.Click += RemoteButton_Click;
            this.AutoScaleMode = AutoScaleMode.Inherit;
            
            remote = r ?? throw new ArgumentNullException("Remote cannot be null");
            remote.RemoteUpdated += UpdateInfo;
            Disposed += OnDisposed;
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            Disposed -= OnDisposed;
            remote.RemoteUpdated -= UpdateInfo;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            scale = factor;
        }

        private void RemoteButton_MouseDown(object sender, MouseEventArgs e) => mouseDown = true;
        private void RemoteButton_MouseUp(object sender, MouseEventArgs e) => mouseDown = false;
        private void RemoteButton_MouseEnter(object sender, EventArgs e) { mouseHover = true; Invalidate();  }
        private void RemoteButton_MouseLeave(object sender, EventArgs e) { mouseHover = false; Invalidate(); }

        public void UpdateInfo(object s, EventArgs a)
        {
            Invalidate();
        }

        private void RemoteButton_Click(object sender, EventArgs e)
        {
            remote.ProcessSendToTransfer();
            remote.OpenWindow();
        }

        private void DrawButton(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            float sw = scale.Width;
            float sh = scale.Height;
            var border = new Pen(SystemColors.ControlDark, 2);
            Brush clr = remote.IncomingTransferFlag ? Brushes.CornflowerBlue : SystemBrushes.ControlLight;
            if (mouseDown) clr = Brushes.LightGray;
            else if (mouseHover) clr = SystemBrushes.Control;
            g.FillRectangle(clr, e.ClipRectangle);
            g.DrawRectangle(border, e.ClipRectangle);
            if (remote.Picture != null)
                g.DrawImage(remote.Picture, 8 * sw, 8 * sh, 48 * sw, 48 * sh);
            g.DrawString(remote.DisplayName, SystemFonts.DefaultFont, SystemBrushes.ControlText, 68 * sw, 8 * sh);
            g.DrawString(remote.UserName + "@" + remote.Hostname, SystemFonts.DefaultFont, SystemBrushes.ControlText, 68 * sw, 28 * sh);
            g.DrawString(remote.Address + ":" + remote.Port, SystemFonts.DefaultFont, SystemBrushes.ControlText, 68 * sw, 48 * sh);
            Image statusImg = null;
            switch (remote.Status)
            {
                case RemoteStatus.AWAITING_DUPLEX:
                    statusImg = Properties.Resources.awaiting_duplex; break;
                case RemoteStatus.CONNECTING:
                    statusImg = Properties.Resources.connecting; break;
                case RemoteStatus.CONNECTED:
                    statusImg = Properties.Resources.connected; break;
                case RemoteStatus.DISCONNECTED:
                    statusImg = Properties.Resources.disconnected; break;
                case RemoteStatus.ERROR:
                    statusImg = Properties.Resources.error; break;
            }
            if ((remote.Status == RemoteStatus.DISCONNECTED || remote.Status == RemoteStatus.ERROR) && !remote.ServiceAvailable)
                statusImg = Properties.Resources.invisible;
            if (statusImg != null)
                g.DrawImage(statusImg, e.ClipRectangle.Width - 48 * sw, 16 * sh, 32 * sw, 32 * sh);
        }
    }
}
