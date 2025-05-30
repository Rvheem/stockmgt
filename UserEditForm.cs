using System;
using System.Windows.Forms;
using StockManagementApp.Models;
using System.ComponentModel;

namespace StockManagementApp.Modules
{
    public partial class UserEditForm : Form
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public User User { get; private set; }
        private bool _isNewUser;

        public UserEditForm(User user = null)
        {
            InitializeComponent();
            
            // Initialize with a new user if null was passed
            User = user ?? new User();
            
            if (user == null)
            {
                // Create new user
                _isNewUser = true;
                this.Text = "Add New User";
            }
            else
            {
                // Edit existing user
                _isNewUser = false;
                this.Text = "Edit User";
            }

            // Populate the form with user data if editing
            if (!_isNewUser)
            {
                txtUsername.Text = User.Username;
                // Don't populate password field for security reasons
                cmbRole.Text = User.Role;
                // Removed: txtEmail.Text = User.Email;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate form inputs
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show("Username is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                if (_isNewUser && string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Password is required for new users.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(cmbRole.Text))
                {
                    MessageBox.Show("Role is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbRole.Focus();
                    return;
                }

                // Update the user object with form values
                User.Username = txtUsername.Text;
                
                // Only update password if provided (for editing existing users)
                if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    // In a real application, you would hash the password
                    User.PasswordHash = txtPassword.Text;
                }
                
                // When saving a user
                User.Username = txtUsername.Text;
                User.PasswordHash = txtPassword.Text; // (if provided)
                User.Role = cmbRole.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
