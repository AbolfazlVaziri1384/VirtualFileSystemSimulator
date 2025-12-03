using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VirtualFileSystemSimulatorWinForm
{
    public partial class RegisterationForm : Form
    {
        public RegisterationForm()
        {
            InitializeComponent();
        }

        private void btnRegisteration_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text.Trim() != txtReapetPassword.Text.Trim())
            {
                MessageBox.Show("Password and RepeatPassword is not the same", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Json user = new Json();
            user.Register(txtUsername.Text.Trim(), txtPassword.Text.Trim(), User.UserTypeEnum.Owner);
            MessageBox.Show($"Registration is Successfully", "SUCCESS", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            Close();
        }

        private void RegisterationForm_Load(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = true;
            txtReapetPassword.UseSystemPasswordChar = true;
        }
    }
}
