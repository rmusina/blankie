using System;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Net.Sockets;

namespace Blankie
{
    public partial class MainForm : Form
    {
        private Streamer streamer;

        private bool isSharing = false;

        private string ip;
        private int port = 80;

        public MainForm()
        {
            InitializeComponent();

            TopMost = true;

            StartPosition = FormStartPosition.Manual;
            int x = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
            int y = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            Bounds = new Rectangle(x, y, this.Width, this.Height);

            ip = Streamer.GetExternalIp();

            streamer = new Streamer(ip, port);
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            isSharing = !isSharing;

            btnStartStop.ImageIndex = isSharing ? 1 : 0;
            btnStartStop.Text = isSharing ? "Stop sharing" : "Start sharing";

            if (isSharing)
            {
                urlTextBox.Text = streamer.URL;
                streamer.Play();
            }
            else
            {
                streamer.Stop();
            }
        }

        private void shareDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();

            aboutBox.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            shareDesktopToolStripMenuItem_Click(null, null);
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void btnClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(urlTextBox.Text, true);
        }
    }
}
