using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class frmLogin : Form
    {
        IPAddress serverIP = IPAddress.Parse("127.0.0.1");
        int port = 1337;
        bool secondTry = false;

        public frmLogin()
        {
            InitializeComponent();
            Connect();
            txtUsername.Text = Properties.Settings.Default.Username;
            txtPassword.Text = Properties.Settings.Default.Password;
        }

        async void Connect()
        {
            try
            {
                Program.client = new TcpClient();
                await Program.client.ConnectAsync(serverIP, port);
                Program.stream = Program.client.GetStream();

                if (secondTry)
                    MessageBox.Show("Succesfully connected to the server.", "Connection established", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Enabled = true;
            }
            catch (SocketException)
            {
                DialogResult dr = MessageBox.Show("Could not connect to server.\nThe server is probably offline.", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                if (dr == DialogResult.Cancel)
                    Application.Exit();
                else if (dr == DialogResult.Retry)
                {
                    secondTry = true;
                    Connect();
                }
            }
        }

        private void TextChanged(object sender, EventArgs e)
        {
            if (txtUsername.Text != "" && txtPassword.Text != "")
                btnLogin.Enabled = true;
            else
                btnLogin.Enabled = false;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            HandleLogin(txtUsername.Text, txtPassword.Text);
        }

        private async void HandleLogin(string username, string password)
        {
            try
            {
                Program.stream.Flush();
                string message = string.Format("lin/{0}/{1}", username.ToLower(), password);
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                await Program.stream.WriteAsync(buffer, 0, buffer.Length);

                Program.stream.Flush();
                buffer = new byte[4096];
                int bytesRead = await Program.stream.ReadAsync(buffer, 0, buffer.Length);
                string answer = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                if (answer.Substring(0, 3) == "pos")
                {
                    if (cboxRemember.Checked)
                    {
                        Properties.Settings.Default.Username = username;
                        Properties.Settings.Default.Password = password;
                        Properties.Settings.Default.Save();
                    }

                    this.Hide();
                    this.Enabled = true;
                    Program.username = username.Substring(0, 1).ToUpper() + username.Substring(1).ToLower();
                    Form frmChat = new frmChat();
                    frmChat.Show();
                }
                else if (answer.Substring(0, 3) == "neg")
                {
                    MessageBox.Show("Invalid password/username combination or the account is already logged in.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Enabled = true;
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Lost connection to the server.\nApplication will now exit.", "Connection lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form frmRegister = new frmRegister();
            frmRegister.Show();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
