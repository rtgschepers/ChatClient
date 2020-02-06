using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class frmRegister : Form
    {
        public frmRegister()
        {
            InitializeComponent();
            this.Enabled = true;
        }

        private new void TextChanged(object sender, EventArgs e)
        {
            if (txtUsername.Text != "" && txtPassword1.Text != "" && txtPassword2.Text != "")
                btnSubmit.Enabled = true;
            else
                btnSubmit.Enabled = false;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
            Program.frmLogin.Show();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword1.Text;

            if (password == txtPassword2.Text)
                HandleRegistration(username, password);
            else
            {
                MessageBox.Show("The passwords you entered do not match.", "Error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.ActiveControl = txtPassword1;
            }  
        }

        private async void HandleRegistration(string username, string password)
        {
            this.Enabled = false;
            try
            {
                Program.stream.Flush();
                string message = string.Format("reg/{0}/{1}", username.ToLower(), password);
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                await Program.stream.WriteAsync(buffer, 0, buffer.Length);

                Program.stream.Flush();
                buffer = new byte[4096];
                int bytesRead = await Program.stream.ReadAsync(buffer, 0, buffer.Length);
                string answer = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                if (answer == "pos")
                {
                    MessageBox.Show("Registration Succesful", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Hide();
                    Program.frmLogin.Show();
                }
                else if (answer == "neg")
                {
                    MessageBox.Show("Username already taken.", "Redundancy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.ActiveControl = txtUsername;
                    txtUsername.SelectAll();
                    this.Enabled = true;
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Lost connection to the server.\nApplication will now exit.", "Connection lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }
}
