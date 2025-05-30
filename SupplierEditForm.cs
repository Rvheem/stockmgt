using System;
using System.Windows.Forms;
using StockManagementApp.Models;

namespace StockManagementApp
{
    public partial class SupplierEditForm : Form
    {
        private Supplier _supplier;
        private bool _isNewSupplier;

        public SupplierEditForm(Supplier? supplier = null)
        {
            InitializeComponent();
            _isNewSupplier = supplier == null;
            _supplier = supplier ?? new Supplier();
            
            if (!_isNewSupplier)
            {
                // Populate form with existing supplier data
                txtName.Text = _supplier.Name;
                txtContact.Text = _supplier.ContactPerson;
                txtPhone.Text = _supplier.Phone;
                txtEmail.Text = _supplier.Email;
                txtAddress.Text = _supplier.Address;
                
                Text = "Edit Supplier";
            }
            else
            {
                Text = "Add New Supplier";
            }
        }

        public Supplier Supplier => _supplier;

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                _supplier.Name = txtName.Text.Trim();
                _supplier.ContactPerson = txtContact.Text.Trim();
                _supplier.Phone = txtPhone.Text.Trim();
                _supplier.Email = txtEmail.Text.Trim();
                _supplier.Address = txtAddress.Text.Trim();
                _supplier.IsActive = true; // Ensure IsActive is set

                if (_isNewSupplier)
                {
                    _supplier.CreatedAt = DateTime.Now;
                    // Code to save new supplier to database
                    // DataHelper.AddSupplier(_supplier);
                }
                else
                {
                    _supplier.UpdatedAt = DateTime.Now;
                    // Code to update existing supplier in database
                    // DataHelper.UpdateSupplier(_supplier);
                }
                
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Supplier name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Phone number is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Address is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAddress.Focus();
                return false;
            }
            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
