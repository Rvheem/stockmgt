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
        
        public User User => UserToEdit ?? new User 
        { 
            Username = "newuser", 
            Password = "password", 
            FullName = "New User", 
            Role = "User" 
        };
        
        private bool _isNewUser;
        

        // Remove these fields, as they are already defined in the Designer file
        // private TextBox txtUsername = new TextBox();
        // private ComboBox cmbRole = new ComboBox();
        // private TextBox txtPassword = new TextBox();

        // Keep only the extra fields not present in the Designer
        private TextBox txtFullName = new TextBox();
        private CheckBox chkActive = new CheckBox();
        private Label lblConfirmPassword = new Label();
        private TextBox txtConfirmPassword = new TextBox();

        public UserEditForm()
        {
            InitializeComponent();
            _context = DbContextFactory.CreateContext();
            _isNewUser = true;
            UserToEdit = new User();
            // Minimal initialization for controls to avoid null reference errors
            txtFullName.Text = "";
            chkActive.Checked = true;
            lblConfirmPassword.Visible = true;
            txtConfirmPassword.Visible = true;
            txtUsername.Text = "";
            cmbRole.Items.AddRange(new string[] { "Administrator", "Manager", "User" });
            cmbRole.SelectedIndex = 0;
            txtPassword.Text = "";
            LoadRoles();
        }

        public UserEditForm(User? user)
        {
            InitializeComponent();
            _context = new StockContext();
            
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
            
            if (!_isNewUser && UserToEdit != null)
            {
                txtUsername.Text = UserToEdit.Username;
                txtFullName.Text = UserToEdit.FullName;
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
                UserToEdit.Username = txtUsername.Text;
                
                // Only update password if provided (for editing existing users)
                if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    // In a real application, you would hash the password
                    UserToEdit.Password = txtPassword.Text;
                }
                
                // When saving a user
                UserToEdit.Username = txtUsername.Text;
                UserToEdit.Password = txtPassword.Text; // (if provided)
                UserToEdit.Role = cmbRole.Text;

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
