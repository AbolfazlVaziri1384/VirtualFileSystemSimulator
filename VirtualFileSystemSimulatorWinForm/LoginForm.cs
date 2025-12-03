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
            Json users = new Json();
            if (users.Login(txtUsername.Text.Trim(), txtPassword.Text.Trim()))
            {
                MessageBox.Show($"Welcome {txtUsername.Text.Trim()}", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                SystemFile fs = new SystemFile();
                fs.UserManager = users;
                Form1 frm = new Form1();
                frm.Fs = fs;
                frm.Show();
                Hide();
            }
            else
            {
                MessageBox.Show("Username or Password is wrong", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegisteration_Click(object sender, EventArgs e)
        {
            RegisterationForm registerForm = new RegisterationForm();
            registerForm.ShowDialog();
        }
    }
}
