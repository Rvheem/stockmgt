using System;
using System.Linq;
using System.Windows.Forms;
using StockManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace StockManagementApp.Modules
{
    public partial class UsersControl : UserControl
    {
        private StockContext _context = new StockContext();

        public UsersControl()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers(string search = "")
        {
            try
            {
                var users = _context.Users
                    .Where(u => search == "" || u.Username.Contains(search) || u.Role.Contains(search))
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        u.Role,
                        LastActivity = _context.Histories
                            .Where(h => h.UserId == u.UserId)
                            .OrderByDescending(h => h.Date)
                            .Select(h => h.Date)
                            .FirstOrDefault()
                    })
                    .ToList();
                dataGridViewUsers.DataSource = users;
                
                // Format grid for better readability
                if (dataGridViewUsers.Columns.Contains("UserId"))
                {
                    dataGridViewUsers.Columns["UserId"].Visible = false;
                }
                
                if (dataGridViewUsers.Columns.Contains("LastActivity"))
                {
                    dataGridViewUsers.Columns["LastActivity"].DefaultCellStyle.Format = "g"; // Show date and time
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if current user is an administrator
                var mainForm = this.FindForm() as Form1;
                if (mainForm == null || mainForm.CurrentUser == null || mainForm.CurrentUser.Role != "Administrator")
                {
                    MessageBox.Show("Only administrators can add users.", "Access Denied", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                var form = new UserEditForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _context.Users.Add(form.User);
                    _context.SaveChanges();
                    LoadUsers();
                    
                    // Log the action in history
                    LogAction($"Added user: {form.User.Username}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if current user is an administrator
                var mainForm = this.FindForm() as Form1;
                if (mainForm == null || mainForm.CurrentUser == null || mainForm.CurrentUser.Role != "Administrator")
                {
                    MessageBox.Show("Only administrators can edit users.", "Access Denied", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (dataGridViewUsers.CurrentRow != null)
                {
                    int id = (int)dataGridViewUsers.CurrentRow.Cells["UserId"].Value;
                    var user = _context.Users.Find(id);
                    
                    if (user != null)
                    {
                        var form = new UserEditForm(user);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            _context.SaveChanges();
                            LoadUsers();
                            
                            // Log the action in history
                            LogAction($"Updated user: {user.Username}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if current user is an administrator
                var mainForm = this.FindForm() as Form1;
                if (mainForm == null || mainForm.CurrentUser == null || mainForm.CurrentUser.Role != "Administrator")
                {
                    MessageBox.Show("Only administrators can delete users.", "Access Denied", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (dataGridViewUsers.CurrentRow != null)
                {
                    int id = (int)dataGridViewUsers.CurrentRow.Cells["UserId"].Value;
                    var user = _context.Users.Find(id);
                    
                    if (user != null)
                    {
                        // Don't allow deleting the current user
                        if (user.UserId == mainForm.CurrentUser.UserId)
                        {
                            MessageBox.Show("You cannot delete your own account while logged in.", 
                                "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        // Check if this is the last administrator
                        if (user.Role == "Administrator" && 
                            _context.Users.Count(u => u.Role == "Administrator") <= 1)
                        {
                            MessageBox.Show("Cannot delete the last administrator account.", 
                                "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        var result = MessageBox.Show($"Are you sure you want to delete user '{user.Username}'?", 
                            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            
                        if (result == DialogResult.Yes)
                        {
                            // Update history entries to null user reference
                            var userHistory = _context.Histories.Where(h => h.UserId == id).ToList();
                            foreach (var history in userHistory)
                            {
                                // You might want to handle this differently based on your requirements
                                // For example, you could set it to the admin user's ID instead
                                history.UserId = mainForm.CurrentUser.UserId;
                                history.Action += $" (Original user: {user.Username})";
                            }
                            
                            _context.Users.Remove(user);
                            _context.SaveChanges();
                            LoadUsers();
                            
                            // Log the action in history
                            LogAction($"Deleted user: {user.Username}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadUsers(txtSearch.Text);
        }
        
        private void LogAction(string action)
        {
            try
            {
                // Get the current user ID from the parent form
                var mainForm = this.FindForm() as Form1;
                if (mainForm != null && mainForm.CurrentUser != null)
                {
                    var historyEntry = new History
                    {
                        Action = action,
                        Date = DateTime.Now,
                        UserId = mainForm.CurrentUser.UserId
                    };
                    
                    _context.Histories.Add(historyEntry);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Just log to console for now, don't disrupt the UI                System.Diagnostics.Debug.WriteLine($"Error logging action: {ex.Message}");
            }
        }
    }
}
