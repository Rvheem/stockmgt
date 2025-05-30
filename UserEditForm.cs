using System;
using System.Windows.Forms;
using StockManagementApp.Models;
using System.ComponentModel;

namespace StockManagementApp.Modules
{
    public partial class UserEditForm : Form
    {
        private StockContext _context;
        
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public User? UserToEdit { get; set; }
          [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public User User => UserToEdit;
          private bool _isNewUser;        // All form controls are defined in the Designer file
        // No need to redefine them here
        public UserEditForm()
        {
            InitializeComponent();
            _context = DbContextFactory.CreateContext();
            _isNewUser = true;
            
            // Initialize with default values for required fields
            UserToEdit = new User 
            { 
                Username = "", 
                Password = "", 
                FullName = "", 
                Role = "User",
                IsActive = true
            };
              // Initialize form controls
            txtFullName.Text = "";
            chkActive.Checked = true;
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtConfirmPassword.Text = "";
            
            // Ensure confirm password fields are visible for new users
            lblConfirmPassword.Visible = true;
            txtConfirmPassword.Visible = true;
            
            LoadRoles();
        }        public UserEditForm(User? user)
        {
            InitializeComponent();
            _context = DbContextFactory.CreateContext(); // Use factory for consistent context creation
            
            // Create new user or use existing one
            if (user == null)
            {
                // Initialize with default values for required fields
                UserToEdit = new User 
                { 
                    Username = "", 
                    Password = "", 
                    FullName = "", 
                    Role = "User",
                    IsActive = true
                };
                _isNewUser = true;
                this.Text = "Add New User";
            }
            else
            {
                UserToEdit = user;
                _isNewUser = false;
                this.Text = "Edit User";
            }
              LoadRoles();
            
            if (UserToEdit != null)
            {
                txtUsername.Text = UserToEdit.Username;
                txtFullName.Text = UserToEdit.FullName ?? "";
                cmbRole.Text = UserToEdit.Role;
                chkActive.Checked = UserToEdit.IsActive;
                
                // Only show password fields for new users
                if (!_isNewUser)
                {
                    lblPassword.Visible = false;
                    txtPassword.Visible = false;
                    lblConfirmPassword.Visible = false;
                    txtConfirmPassword.Visible = false;
                }
            }
        }

        private void LoadRoles()
        {
            // Example roles, adjust as needed
            cmbRole.Items.Clear();
            cmbRole.Items.AddRange(new string[] { "Administrator", "Manager", "User" });
            cmbRole.SelectedIndex = 0;
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
                
                // Check if username is unique (only for new users or when username changed)
                bool usernameChanged = !_isNewUser && UserToEdit.Username != txtUsername.Text;
                if ((_isNewUser || usernameChanged) && _context.Users.Any(u => u.Username == txtUsername.Text))
                {
                    MessageBox.Show($"A user with username '{txtUsername.Text}' already exists. Please choose a different username.", 
                        "Duplicate Username", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }
                
                if (_isNewUser && string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Password is required for new users.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                // Check if password and confirmed password match for new users
                if (_isNewUser && txtPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("Password and confirm password do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassword.Focus();
                    return;
                }

                // Validate full name
                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    MessageBox.Show("Full name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFullName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(cmbRole.Text))
                {
                    MessageBox.Show("Role is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbRole.Focus();
                    return;
                }
                
                // Update the user object with form values
                UserToEdit.Username = txtUsername.Text;
                UserToEdit.Role = cmbRole.Text;
                UserToEdit.FullName = txtFullName.Text;
                UserToEdit.IsActive = chkActive.Checked;
                
                // Only update password if provided (for editing existing users)
                if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    // In a real application, you would hash the password
                    UserToEdit.Password = txtPassword.Text;
                }
                else if (_isNewUser)
                {
                    // Ensure new users always have a password
                    MessageBox.Show("Password is required for new users.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user: {ex.Message}\n\nStack Trace: {ex.StackTrace}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
