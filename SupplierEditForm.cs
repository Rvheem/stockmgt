using System;
using System.Windows.Forms;
using StockManagementApp.Models;
using System.ComponentModel;

namespace StockManagementApp.Modules
{
    public partial class SupplierEditForm : Form
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Supplier Supplier { get; private set; }
        private bool _isNewSupplier;

        public SupplierEditForm(Supplier supplier = null)
        {
            InitializeComponent();
            
            // Initialize with a new supplier if null was passed
            Supplier = supplier ?? new Supplier();
            
            if (supplier == null)
            {
                // Create new supplier
                _isNewSupplier = true;
                this.Text = "Add New Supplier";
            }
            else
            {
                // Edit existing supplier
                _isNewSupplier = false;
                this.Text = "Edit Supplier";
            }

            // Populate the form with supplier data if editing
            if (!_isNewSupplier)
            {
                txtName.Text = Supplier.Name;
                txtPhone.Text = Supplier.Phone;
                txtAddress.Text = Supplier.Address;
                txtEmail.Text = Supplier.Email;
                txtContactPerson.Text = Supplier.ContactPerson;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate form inputs
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Supplier name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtName.Focus();
                    return;
                }

                // Update the supplier object with form values
                Supplier.Name = txtName.Text;
                Supplier.Phone = txtPhone.Text;
                Supplier.Address = txtAddress.Text;
                Supplier.Email = txtEmail.Text;
                Supplier.ContactPerson = txtContactPerson.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving supplier: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
