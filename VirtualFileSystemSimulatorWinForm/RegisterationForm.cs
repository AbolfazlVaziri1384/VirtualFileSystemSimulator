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
            try
            {
                if (txtPassword.Text.Trim() != txtReapetPassword.Text.Trim())
                {
                    MessageBox.Show("Password and confirmation password do not match. Please try again.",
                                   "Password Mismatch",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    txtPassword.SelectAll();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show("Please enter a username.",
                                   "Username Required",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                Json user = new Json();

                user.Register(txtUsername.Text.Trim(), txtPassword.Text.Trim());

                MessageBox.Show("Registration successful! You can now log in to your account.",
                               "Registration Complete",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Information);

                Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Invalid input: {ex.Message}\nPlease check your information and try again.",
                               "Input Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Registration failed: {ex.Message}\nPlease try a different username or contact support.",
                               "Registration Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Show($"Unable to save registration data: {ex.Message}\nPlease check available storage and try again.",
                               "Storage Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Permission denied: {ex.Message}\nPlease check your file permissions.",
                               "Access Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred during registration: {ex.Message}\nPlease try again or contact support if the problem persists.",
                               "System Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
        }

        private void RegisterationForm_Load(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = true;
            txtReapetPassword.UseSystemPasswordChar = true;
        }
    }
}
