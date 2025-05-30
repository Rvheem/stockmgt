using System;
using System.Linq;
using System.Windows.Forms;
using StockManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace StockManagementApp.Modules
{
    public partial class HistoryControl : UserControl
    {
        private StockContext _context = new StockContext();

        public HistoryControl()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void LoadHistory(string search = "", DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Histories.AsQueryable();
                
                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(h => h.Action.Contains(search) || h.User.Username.Contains(search));
                }
                
                // Apply date filters
                if (startDate.HasValue)
                {
                    query = query.Where(h => h.Date >= startDate.Value);
                }
                
                if (endDate.HasValue)
                {
                    query = query.Where(h => h.Date <= endDate.Value.AddDays(1)); // Include the end date fully
                }
                
                var history = query
                    .OrderByDescending(h => h.Date)
                    .Select(h => new
                    {
                        h.HistoryId,
                        h.Action,
                        h.Date,
                        User = h.User.Username
                    })
                    .ToList();
                    
                dataGridViewHistory.DataSource = history;
                
                // Format grid for better readability
                if (dataGridViewHistory.Columns.Contains("HistoryId"))
                {
                    dataGridViewHistory.Columns["HistoryId"].Visible = false;
                }
                
                if (dataGridViewHistory.Columns.Contains("Date"))
                {
                    dataGridViewHistory.Columns["Date"].DefaultCellStyle.Format = "g"; // Show date and time
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string search = txtSearch.Text;
                DateTime? startDate = dtpStartDate.Checked ? dtpStartDate.Value : null;
                DateTime? endDate = dtpEndDate.Checked ? dtpEndDate.Value : null;
                
                LoadHistory(search, startDate, endDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                // Only administrators should be able to clear history
                var mainForm = this.FindForm() as Form1;
                if (mainForm == null || mainForm.CurrentUser == null || mainForm.CurrentUser.Role != "Administrator")
                {
                    MessageBox.Show("Only administrators can clear history records.", "Access Denied", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                var result = MessageBox.Show("Are you sure you want to clear all history records? This action cannot be undone.", 
                    "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    
                if (result == DialogResult.Yes)
                {
                    // Double-check
                    var confirm = MessageBox.Show("This will permanently delete ALL history records. Are you absolutely sure?", 
                        "Final Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        
                    if (confirm == DialogResult.Yes)
                    {
                        // Clear history but save the clear action itself
                        var historyEntry = new History
                        {
                            Action = "Cleared all history records",
                            Date = DateTime.Now,
                            UserId = mainForm.CurrentUser.UserId
                        };
                        
                        // Get all except the most recent (which would be the login record)
                        var historyToDelete = _context.Histories.ToList();
                        _context.Histories.RemoveRange(historyToDelete);
                        
                        // Add the clear action
                        _context.Histories.Add(historyEntry);
                        _context.SaveChanges();
                        
                        LoadHistory();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                    saveDialog.DefaultExt = "csv";
                    saveDialog.FileName = $"HistoryExport_{DateTime.Now:yyyyMMdd}";
                    
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Create CSV content
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        
                        // Add header
                        sb.AppendLine("Action,Date,User");
                        
                        // Add rows
                        foreach (DataGridViewRow row in dataGridViewHistory.Rows)
                        {
                            string action = row.Cells["Action"].Value.ToString();
                            string date = row.Cells["Date"].Value.ToString();
                            string user = row.Cells["User"].Value.ToString();
                            
                            // Escape any commas in the action
                            if (action.Contains(","))
                            {
                                action = $"\"{action}\"";
                            }
                            
                            sb.AppendLine($"{action},{date},{user}");
                        }
                        
                        // Write to file
                        System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString());
                        
                        MessageBox.Show($"History exported successfully to {saveDialog.FileName}", "Export Complete", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting history: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
