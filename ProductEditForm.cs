using System;
using System.Linq;
using System.Windows.Forms;
using StockManagementApp.Models;
using System.ComponentModel;

namespace StockManagementApp.Modules
{
    public partial class ProductEditForm : Form
    {
        private StockContext _context = new StockContext();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Product Product { get; private set; }
        private bool _isNewProduct;

        public ProductEditForm(Product product = null)
        {
            InitializeComponent();
            
            // Load suppliers for the dropdown
            LoadSuppliers();
            
            // Initialize with a new product if null was passed
            Product = product ?? new Product();
            
            if (product == null)
            {
                // Create new product
                _isNewProduct = true;
                this.Text = "Add New Product";
            }
            else
            {
                // Edit existing product
                _isNewProduct = false;
                this.Text = "Edit Product";
            }

            // Populate the form with product data if editing
            if (!_isNewProduct)
            {
                txtName.Text = Product.Name;
                txtReference.Text = Product.Reference;
                numQuantity.Value = Product.Quantity;
                numThreshold.Value = Product.Threshold;
                numPrice.Value = (decimal)Product.Price;
                dtpExpiryDate.Value = Product.ExpiryDate ?? DateTime.Now.AddYears(1);
                dtpExpiryDate.Checked = Product.ExpiryDate.HasValue;
                
                if (Product.SupplierId.HasValue)
                {
                    cmbSupplier.SelectedValue = Product.SupplierId.Value;
                }
            }
        }
        
        private void LoadSuppliers()
        {
            try
            {
                var suppliers = _context.Suppliers.ToList();
                cmbSupplier.DataSource = suppliers;
                cmbSupplier.DisplayMember = "Name";
                cmbSupplier.ValueMember = "SupplierId";
                cmbSupplier.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate form inputs
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Product name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtName.Focus();
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtReference.Text))
                {
                    MessageBox.Show("Product reference is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtReference.Focus();
                    return;
                }

                // Update the product object with form values
                Product.Name = txtName.Text;
                Product.Reference = txtReference.Text;
                Product.Quantity = (int)numQuantity.Value;
                Product.Threshold = (int)numThreshold.Value;
                Product.Price = (decimal)numPrice.Value;
                Product.ExpiryDate = dtpExpiryDate.Checked ? dtpExpiryDate.Value : (DateTime?)null;
                
                if (cmbSupplier.SelectedValue != null)
                {
                    Product.SupplierId = (int)cmbSupplier.SelectedValue;
                }
                else
                {
                    Product.SupplierId = null;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
