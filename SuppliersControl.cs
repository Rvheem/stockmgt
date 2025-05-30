using System;
using System.Linq;
using System.Windows.Forms;
using StockManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace StockManagementApp.Modules
{
    public partial class SuppliersControl : UserControl
    {
        private StockContext _context = new StockContext();

        public SuppliersControl()
        {
            InitializeComponent();
            LoadSuppliers();
        }

        private void LoadSuppliers(string search = "")
        {
            try
            {
                var suppliers = _context.Suppliers
                    .Where(s => search == "" || s.Name.Contains(search) || s.Phone.Contains(search))
                    .Select(s => new
                    {
                        s.SupplierId,
                        s.Name,
                        s.Phone,
                        s.Address,
                        ProductCount = s.Products.Count
                    })
                    .ToList();
                dataGridViewSuppliers.DataSource = suppliers;
                
                // Format the grid for better readability
                if (dataGridViewSuppliers.Columns.Contains("SupplierId"))
                {
                    dataGridViewSuppliers.Columns["SupplierId"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var form = new SupplierEditForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _context.Suppliers.Add(form.Supplier);
                    _context.SaveChanges();
                    LoadSuppliers();
                    
                    // Log the action in history
                    LogAction($"Added supplier: {form.Supplier.Name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding supplier: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewSuppliers.CurrentRow != null)
                {
                    int id = (int)dataGridViewSuppliers.CurrentRow.Cells["SupplierId"].Value;
                    var supplier = _context.Suppliers.Find(id);
                    
                    if (supplier != null)
                    {
                        var form = new SupplierEditForm(supplier);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            _context.SaveChanges();
                            LoadSuppliers();
                            
                            // Log the action in history
                            LogAction($"Updated supplier: {supplier.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing supplier: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewSuppliers.CurrentRow != null)
                {
                    int id = (int)dataGridViewSuppliers.CurrentRow.Cells["SupplierId"].Value;
                    var supplier = _context.Suppliers.Find(id);
                    
                    if (supplier != null)
                    {
                        // Check if supplier has products
                        var hasProducts = _context.Products.Any(p => p.SupplierId == id);
                        
                        if (hasProducts)
                        {
                            MessageBox.Show("Cannot delete supplier with existing products.", "Delete Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        var result = MessageBox.Show($"Are you sure you want to delete supplier '{supplier.Name}'?", 
                            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            
                        if (result == DialogResult.Yes)
                        {
                            _context.Suppliers.Remove(supplier);
                            _context.SaveChanges();
                            LoadSuppliers();
                            
                            // Log the action in history
                            LogAction($"Deleted supplier: {supplier.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting supplier: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadSuppliers(txtSearch.Text);
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
                // Just log to console for now, don't disrupt the UI
                System.Diagnostics.Debug.WriteLine($"Error logging action: {ex.Message}");
            }
        }
    }
}
