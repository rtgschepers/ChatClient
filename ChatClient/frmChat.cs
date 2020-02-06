using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class frmChat : Form
    {
        NotifyIcon notifyIcon = new NotifyIcon();
        public frmChat()
        {
            InitializeComponent();
        }

        private void frmChat_OnLoad(object sender, EventArgs e)
        {
            lblUsername.Text = "Hello " + Program.username;
            this.ActiveControl = txtMessage;
            SetBalloonTip();
            Task.Factory.StartNew(ListenForData);
        }

        async void ListenForData()
        {
            try
            {
                while (this.Visible)
                {
                    Program.stream.Flush();
                    byte[] buffer = new byte[4096];
                    int bytesRead = await Program.stream.ReadAsync(buffer, 0, buffer.Length);
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    if (message.Substring(0, 3) == "cht")
                        UpdateChat(message.Substring(4));
                    else if (message.Substring(0, 3) == "usr")
                        GetUserList(message);
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Lost connection to the server.\nApplication will now exit.", "Connection lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        void SetBalloonTip()
        {
            notifyIcon.Icon = SystemIcons.Information;
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.BalloonTipTitle = "You've been mentioned!";
        }

        void GetUserList(string data)
        {
            ClearUserList();
            string[] usernames = data.Split('/');
            for (int i = 1; i < usernames.Length - 1; i++)
                UpdateUserList(usernames[i]);
        }

        void ClearUserList()
        {
            if (lbUsers.InvokeRequired)
            {
                lbUsers.Invoke(new Action(ClearUserList));
                return;
            }
            lbUsers.Items.Clear();
        }

        void UpdateUserList(string username)
        {
            if (lbUsers.InvokeRequired)
            {
                lbUsers.Invoke(new Action<string>(UpdateUserList), username);
                return;
            }
            lbUsers.Items.Add(username);
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMessage.Text != "" && txtMessage.Text != "Type here...")
            {
                string command = string.Format("cht/{0}", txtMessage.Text);
                txtMessage.Text = "";

                Program.stream.Flush();
                byte[] buffer = Encoding.ASCII.GetBytes(command);
                await Program.stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        void UpdateChat(string message)
        {
            if (rtbChat.InvokeRequired)
            {
                rtbChat.Invoke(new Action<string>(UpdateChat), message);
                return;
            }

            rtbChat.Text += string.Format("{0}\n", message);
            rtbChat.ScrollToCaret();

            if (message.ToLower().Contains(Program.username.ToLower()))
            {
                if (!this.ContainsFocus)
                {
                    notifyIcon.Visible = true;
                    notifyIcon.BalloonTipText = message;
                    notifyIcon.ShowBalloonTip(1000);
                }
            }
        }

        async void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Program.stream.Flush();
                string command = string.Format("out/{0}", Program.username.ToLower());
                byte[] buffer = Encoding.ASCII.GetBytes(command);
                await Program.stream.WriteAsync(buffer, 0, buffer.Length);
                Application.Exit();
            }
            catch (Exception)
            {
                MessageBox.Show("Lost connection to the server.\nApplication will now exit.", "Connection lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        public void TextGotFocus(object sender, EventArgs e)
        {
            if (txtMessage.Text == "Type here...")
            {
                txtMessage.Text = "";
                txtMessage.ForeColor = Color.Black;
            }
        }

        public void TextLostFocus(object sender, EventArgs e)
        {
            if (txtMessage.Text == "")
            {
                txtMessage.Text = "Type here...";
                txtMessage.ForeColor = Color.Gray;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            rtbChat.Text = "***[CHAT CLEARED]***\n";
            this.ActiveControl = txtMessage;
        }
    }
}
