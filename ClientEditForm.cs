using System;
using System.ComponentModel;
using System.Windows.Forms;
using StockManagementApp.Models;

namespace StockManagementApp.Modules
{
    public partial class ClientEditForm : Form
    {
        private StockContext _context;
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Client Client { get; private set; }
        
        private TextBox txtName;
        private TextBox txtPhone;
        private TextBox txtAddress;
        private Button btnSave;
        private Button btnCancel;
        
        public ClientEditForm() : this(null)
        {
        }
        
        public ClientEditForm(Client? client)
        {
            InitializeComponent();
            _context = DbContextFactory.CreateContext();
            
            // Initialize with a new client if null was passed
            Client = client ?? new Client();
            
            if (client != null)
            {
                // Populate form fields with client data
                txtName.Text = Client.Name;
                txtPhone.Text = Client.Phone ?? string.Empty;
                txtAddress.Text = Client.Address ?? string.Empty;
                
                this.Text = "Edit Client";
            }
            else
            {
                this.Text = "Add New Client";
            }
        }
        
        private void InitializeComponent()
        {
            this.txtName = new TextBox();
            this.txtPhone = new TextBox();
            this.txtAddress = new TextBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            
            // Form properties
            this.ClientSize = new System.Drawing.Size(400, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            
            // Name
            Label lblName = new Label();
            lblName.AutoSize = true;
            lblName.Location = new System.Drawing.Point(20, 25);
            lblName.Text = "Name:";
            
            this.txtName.Location = new System.Drawing.Point(120, 22);
            this.txtName.Size = new System.Drawing.Size(250, 23);
            
            // Phone
            Label lblPhone = new Label();
            lblPhone.AutoSize = true;
            lblPhone.Location = new System.Drawing.Point(20, 65);
            lblPhone.Text = "Phone:";
            
            this.txtPhone.Location = new System.Drawing.Point(120, 62);
            this.txtPhone.Size = new System.Drawing.Size(250, 23);
            
            // Address
            Label lblAddress = new Label();
            lblAddress.AutoSize = true;
            lblAddress.Location = new System.Drawing.Point(20, 105);
            lblAddress.Text = "Address:";
            
            this.txtAddress.Location = new System.Drawing.Point(120, 102);
            this.txtAddress.Size = new System.Drawing.Size(250, 23);
            this.txtAddress.Multiline = true;
            this.txtAddress.Height = 60;
            
            // Buttons
            this.btnSave.Location = new System.Drawing.Point(120, 190);
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            
            this.btnCancel.Location = new System.Drawing.Point(270, 190);
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            
            // Add controls to form
            this.Controls.Add(lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(lblPhone);
            this.Controls.Add(this.txtPhone);
            this.Controls.Add(lblAddress);
            this.Controls.Add(this.txtAddress);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate form inputs
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Client name is required.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtName.Focus();
                    return;
                }
                
                // Update the client object with form values
                Client.Name = txtName.Text;
                Client.Phone = txtPhone.Text;
                Client.Address = txtAddress.Text;
                
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving client: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
