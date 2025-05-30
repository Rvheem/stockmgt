using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using StockManagementApp.Models;

namespace StockManagementApp
{
    public partial class LoginForm : Form
    {
        private StockContext _context = new StockContext();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public User User { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }


private void btnLogin_Click(object sender, EventArgs e)
{
    try
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Please enter both username and password.", "Login Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Case-insensitive username check
        var user = _context.Users
            .FirstOrDefault(u => u.Username.ToLower() == username.ToLower() && u.Password == password);

        if (user != null)
        {
            User = user;
            
            // Log the login action
            _context.Histories.Add(new History
            {
                UserId = user.UserId,
                Action = "User logged in",
                Date = DateTime.Now
            });
            _context.SaveChanges();
            
            DialogResult = DialogResult.OK;
        }
        else
        {
            MessageBox.Show("Invalid username or password.", "Login Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}


        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Application.Exit();
        }
    }
}
