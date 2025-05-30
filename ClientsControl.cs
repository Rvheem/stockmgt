using System;
using System.Linq;
using System.Windows.Forms;
using StockManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace StockManagementApp.Modules
{
    public partial class ClientsControl : UserControl
    {
        private StockContext _context = new StockContext();

        public ClientsControl()
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients(string search = "")
        {
            try
            {
                var clients = _context.Clients
                    .Where(c => search == "" || c.Name.Contains(search) || c.Phone.Contains(search))
                    .Select(c => new
                    {
                        c.ClientId,
                        c.Name,
                        c.Phone,
                        c.Address
                    })
                    .ToList();
                dataGridViewClients.DataSource = clients;
                
                // Format the grid for better readability
                if (dataGridViewClients.Columns.Contains("ClientId"))
                {
                    dataGridViewClients.Columns["ClientId"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading clients: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var form = new ClientEditForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _context.Clients.Add(form.Client);
                    _context.SaveChanges();
                    LoadClients();
                    
                    // Log the action in history
                    LogAction($"Added client: {form.Client.Name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding client: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewClients.CurrentRow != null)
                {
                    int id = (int)dataGridViewClients.CurrentRow.Cells["ClientId"].Value;
                    var client = _context.Clients.Find(id);
                    
                    if (client != null)
                    {
                        var form = new ClientEditForm(client);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            _context.SaveChanges();
                            LoadClients();
                            
                            // Log the action in history
                            LogAction($"Updated client: {client.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing client: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewClients.CurrentRow != null)
                {
                    int id = (int)dataGridViewClients.CurrentRow.Cells["ClientId"].Value;
                    var client = _context.Clients.Find(id);
                    
                    if (client != null)
                    {
                        // Check if client has orders
                        var hasOrders = _context.Orders.Any(o => o.ClientId == id);
                        
                        if (hasOrders)
                        {
                            MessageBox.Show("Cannot delete client with existing orders.", "Delete Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        var result = MessageBox.Show($"Are you sure you want to delete client '{client.Name}'?", 
                            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            
                        if (result == DialogResult.Yes)
                        {
                            _context.Clients.Remove(client);
                            _context.SaveChanges();
                            LoadClients();
                            
                            // Log the action in history
                            LogAction($"Deleted client: {client.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting client: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadClients(txtSearch.Text);
        }

        private void btnInvoices_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewClients.CurrentRow != null)
                {
                    int id = (int)dataGridViewClients.CurrentRow.Cells["ClientId"].Value;
                    var client = _context.Clients.Find(id);
                    
                    if (client != null)
                    {
                        var clientOrders = _context.Orders
                            .Where(o => o.ClientId == id)
                            .OrderByDescending(o => o.OrderDate)
                            .ToList();
                            
                        if (clientOrders.Any())
                        {
                            // Show invoices in a simple dialog for now
                            var sb = new System.Text.StringBuilder();
                            sb.AppendLine($"Invoices for {client.Name}:");
                            sb.AppendLine("----------------------------------------");
                            
                            foreach (var order in clientOrders)
                            {
                                sb.AppendLine($"Invoice #{order.OrderId} - Date: {order.OrderDate:d} - Total: ${order.Total:F2}");
                            }
                            
                            MessageBox.Show(sb.ToString(), "Client Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("This client has no orders/invoices.", "Client Invoices", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving invoices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPayments_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewClients.CurrentRow != null)
                {
                    int id = (int)dataGridViewClients.CurrentRow.Cells["ClientId"].Value;
                    var client = _context.Clients.Find(id);
                    
                    if (client != null)
                    {
                        MessageBox.Show("Payment management functionality will be implemented in the next update.", 
                            "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            {                // Just log to console for now, don't disrupt the UI
                System.Diagnostics.Debug.WriteLine($"Error logging action: {ex.Message}");
            }
        }
    }
}
