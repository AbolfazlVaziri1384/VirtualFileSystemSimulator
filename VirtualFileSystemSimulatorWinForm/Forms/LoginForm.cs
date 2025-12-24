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

namespace VirtualFileSystemSimulatorWinForm
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = true;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                Json users = new Json();

                if (users.Login(txtUsername.Text.Trim(), txtPassword.Text.Trim()))
                {
                    MessageBox.Show($"Welcome, {txtUsername.Text.Trim()}! Login successful.",
                                   "Welcome",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Information);

                    SystemFile fs = new SystemFile(users);
                    fs.UserManager = users;
                    Form1 frm = new Form1();
                    frm.Fs = fs;
                    frm.Show();
                    Hide();
                }
                else
                {
                    MessageBox.Show("Invalid username or password. Please try again.",
                                   "Login Failed",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}",
                               "Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
        }

        private void btnRegisteration_Click(object sender, EventArgs e)
        {
            RegisterationForm registerForm = new RegisterationForm();
            registerForm.ShowDialog();
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
    }
}
