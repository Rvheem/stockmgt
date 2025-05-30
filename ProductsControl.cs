using System;
using System.Linq;
using System.Windows.Forms;
using StockManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace StockManagementApp.Modules
{
    public partial class ProductsControl : UserControl
    {
        private StockContext _context = new StockContext();

        public ProductsControl()
        {
            InitializeComponent();
            LoadProducts();
            CheckLowStockAndExpiryAlerts();
        }

        private void LoadProducts(string search = "")
        {
            try
            {
                var products = _context.Products
                    .Where(p => search == "" || 
                           p.Name.Contains(search) || 
                           p.Reference.Contains(search) ||
                           (p.ExpiryDate.HasValue && p.ExpiryDate.Value.ToString().Contains(search)))
                    .Select(p => new
                    {
                        p.ProductId,
                        p.Name,
                        p.Reference,
                        p.Quantity,
                        p.Threshold,
                        ExpiryDate = p.ExpiryDate.HasValue ? p.ExpiryDate.Value.ToShortDateString() : "N/A",
                        Status = GetProductStatus(p)
                    })
                    .ToList();
                dataGridViewProducts.DataSource = products;
                
                // Format the grid for better readability
                if (dataGridViewProducts.Columns.Contains("Status"))
                {
                    dataGridViewProducts.Columns["Status"].DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                    foreach (DataGridViewRow row in dataGridViewProducts.Rows)
                    {
                        if (row.Cells["Status"].Value != null)
                        {
                            string status = row.Cells["Status"].Value.ToString();
                            if (status.Contains("Low"))
                            {
                                row.Cells["Status"].Style.BackColor = System.Drawing.Color.Orange;
                            }
                            else if (status.Contains("Expired") || status.Contains("Expiring"))
                            {
                                row.Cells["Status"].Style.BackColor = System.Drawing.Color.Red;
                            }
                            else
                            {
                                row.Cells["Status"].Style.BackColor = System.Drawing.Color.Green;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        private static string GetProductStatus(Product product)
        {
            // Check stock levels
            if (product.Quantity <= product.Threshold)
            {
                // Check expiry
                if (product.ExpiryDate.HasValue)
                {
                    if (product.ExpiryDate.Value <= DateTime.Now)
                    {
                        return "Low Stock & Expired";
                    }
                    else if (product.ExpiryDate.Value <= DateTime.Now.AddDays(7))
                    {
                        return "Low Stock & Expiring Soon";
                    }
                }
                
                return "Low Stock";
            }
            // Check expiry
            else if (product.ExpiryDate.HasValue)
            {
                if (product.ExpiryDate.Value <= DateTime.Now)
                {
                    return "Expired";
                }
                else if (product.ExpiryDate.Value <= DateTime.Now.AddDays(7))
                {
                    return "Expiring Soon";
                }
            }
            
            return "OK";
        }

        private void CheckLowStockAndExpiryAlerts()
        {
            try
            {
                var alertProducts = _context.Products
                    .Where(p => p.Quantity <= p.Threshold || 
                           (p.ExpiryDate.HasValue && p.ExpiryDate <= DateTime.Now.AddDays(7)))
                    .ToList();
                
                if (alertProducts.Any())
                {
                    string alertMessage = "ALERTS:\n\n";
                    
                    foreach (var product in alertProducts)
                    {
                        if (product.Quantity <= product.Threshold)
                        {
                            alertMessage += $"LOW STOCK: {product.Name} (Qty: {product.Quantity}, Threshold: {product.Threshold})\n";
                        }
                        
                        if (product.ExpiryDate.HasValue && product.ExpiryDate.Value <= DateTime.Now)
                        {
                            alertMessage += $"EXPIRED: {product.Name} expired on {product.ExpiryDate.Value.ToShortDateString()}\n";
                        }
                        else if (product.ExpiryDate.HasValue && product.ExpiryDate.Value <= DateTime.Now.AddDays(7))
                        {
                            alertMessage += $"EXPIRING SOON: {product.Name} expires on {product.ExpiryDate.Value.ToShortDateString()}\n";
                        }
                    }
                    
                    MessageBox.Show(alertMessage, "Stock Alerts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking alerts: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var form = new ProductEditForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _context.Products.Add(form.Product);
                    _context.SaveChanges();
                    LoadProducts();
                    
                    // Log the action in history
                    LogAction($"Added product: {form.Product.Name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewProducts.CurrentRow != null)
                {
                    int id = (int)dataGridViewProducts.CurrentRow.Cells["ProductId"].Value;
                    var product = _context.Products.Find(id);
                    
                    if (product == null)
                    {
                        MessageBox.Show("Product not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    var form = new ProductEditForm(product);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        _context.SaveChanges();
                        LoadProducts();
                        
                        // Log the action in history
                        LogAction($"Updated product: {form.Product.Name}");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a product to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewProducts.CurrentRow != null)
                {
                    int id = (int)dataGridViewProducts.CurrentRow.Cells["ProductId"].Value;
                    var product = _context.Products.Find(id);
                    
                    if (product == null)
                    {
                        MessageBox.Show("Product not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    if (MessageBox.Show($"Are you sure you want to delete {product.Name}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        string productName = product.Name;
                        _context.Products.Remove(product);
                        _context.SaveChanges();
                        LoadProducts();
                        
                        // Log the action in history
                        LogAction($"Deleted product: {productName}");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a product to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadProducts(txtSearch.Text);
        }

        private void btnAlerts_Click(object sender, EventArgs e)
        {
            try
            {
                var alerts = _context.Products
                    .Where(p => p.Quantity <= p.Threshold || 
                           (p.ExpiryDate.HasValue && p.ExpiryDate <= DateTime.Now.AddDays(7)))
                    .Select(p => new
                    {
                        p.ProductId,
                        p.Name,
                        p.Reference,
                        p.Quantity,
                        p.Threshold,
                        ExpiryDate = p.ExpiryDate.HasValue ? p.ExpiryDate.Value.ToShortDateString() : "N/A",
                        Status = GetProductStatus(p)
                    })
                    .ToList();
                    
                dataGridViewProducts.DataSource = alerts;
                
                // Format the grid for better readability
                if (dataGridViewProducts.Columns.Contains("Status"))
                {
                    dataGridViewProducts.Columns["Status"].DefaultCellStyle.ForeColor = System.Drawing.Color.White;
                    foreach (DataGridViewRow row in dataGridViewProducts.Rows)
                    {
                        if (row.Cells["Status"].Value != null)
                        {
                            string status = row.Cells["Status"].Value.ToString();
                            if (status.Contains("Low"))
                            {
                                row.Cells["Status"].Style.BackColor = System.Drawing.Color.Orange;
                            }
                            else if (status.Contains("Expired") || status.Contains("Expiring"))
                            {
                                row.Cells["Status"].Style.BackColor = System.Drawing.Color.Red;
                            }
                            else
                            {
                                row.Cells["Status"].Style.BackColor = System.Drawing.Color.Green;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading alerts: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LogAction(string actionDescription)
        {
            try
            {
                // For now, we'll log with a default user ID of 1
                // In a real app, you would use the current logged in user's ID
                var historyEntry = new History
                {
                    Action = actionDescription,
                    Date = DateTime.Now,
                    UserId = 1
                };
                
                _context.Histories.Add(historyEntry);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Just log the error but don't stop the workflow
                Console.WriteLine($"Error logging action: {ex.Message}");
            }
        }
    }
}
