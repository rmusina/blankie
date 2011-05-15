using System;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Blankie
{
    public partial class MainForm : Form
    {
        private Streamer streamer;

        private WebServer server;

        private bool isSharing = false;

        private Thread serverWorker;

        private string ip;
        private int port = 80;
        private int oldPort;

        public MainForm()
        {
            InitializeComponent();

            TopMost = true;

            StartPosition = FormStartPosition.Manual;
            int x = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
            int y = Screen.PrimaryScreen.WorkingArea.Height - this.Height;
            Bounds = new Rectangle(x, y, this.Width, this.Height);

            ip = Streamer.GetExternalIp();
            portTextBox.Text = port.ToString();
            oldPort = port;

            server = new WebServer(ip, "1234");

            serverWorker = new Thread(server.Start);
            serverWorker.Start();

            streamer = new Streamer(ip, port);
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            isSharing = !isSharing;

            btnStartStop.ImageIndex = isSharing ? 1 : 0;
            btnStartStop.Text = isSharing ? "Stop sharing" : "Start sharing";

            if (isSharing)
            {
                if (!Int32.TryParse(portTextBox.Text, out port))
                {
                    port = oldPort;
                    portTextBox.Text = port.ToString();
                }

                if (port != oldPort)
                {
                    streamer = new Streamer(ip, port);
                    oldPort = port;
                }

                urlTextBox.Text = streamer.URL;
                portTextBox.Enabled = false;
                streamer.Play();
            }
            else
            {
                portTextBox.Enabled = true;
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
            streamer.Stop();
            serverWorker.Abort();
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
