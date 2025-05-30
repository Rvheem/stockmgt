using System;
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
        public Product? ProductToEdit { get; set; } = null;
        private bool _isNewProduct;

        // Add this property to fix the error
        public Product Product => ProductToEdit ?? new Product();
        
        // Fix by properly handling the nullable parameter
        public ProductEditForm(Product? product = null)
        {
            InitializeComponent();
            
            // Load suppliers for the dropdown
            LoadSuppliers();
            
            // Initialize with a new product if null was passed
            ProductToEdit = product ?? new Product();
            
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
                txtName.Text = ProductToEdit.Name;
                txtReference.Text = ProductToEdit.Reference;
                numQuantity.Value = ProductToEdit.Quantity;
                numThreshold.Value = ProductToEdit.Threshold;
                numPrice.Value = (decimal)ProductToEdit.Price;
                dtpExpiryDate.Value = ProductToEdit.ExpiryDate ?? DateTime.Now.AddYears(1);
                dtpExpiryDate.Checked = ProductToEdit.ExpiryDate.HasValue;
                
                if (ProductToEdit.SupplierId.HasValue)
                {
                    cmbSupplier.SelectedValue = ProductToEdit.SupplierId.Value;
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
                ProductToEdit.Name = txtName.Text;
                ProductToEdit.Reference = txtReference.Text;
                ProductToEdit.Quantity = (int)numQuantity.Value;
                ProductToEdit.Threshold = (int)numThreshold.Value;
                ProductToEdit.Price = (decimal)numPrice.Value;
                ProductToEdit.ExpiryDate = dtpExpiryDate.Checked ? dtpExpiryDate.Value : (DateTime?)null;
                
                if (cmbSupplier.SelectedValue != null)
                {
                    ProductToEdit.SupplierId = (int)cmbSupplier.SelectedValue;
                }
                else
                {
                    ProductToEdit.SupplierId = null;
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
