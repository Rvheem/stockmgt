using System;
using System.Linq;
using System.Windows.Forms;
using StockManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace StockManagementApp.Modules
{
    public class DeliveryEditForm : Form
    {
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.Browsable(false)]
        public Delivery Delivery { get; private set; }
        
        private ComboBox cmbOrders;
        private DateTimePicker dtpDeliveryDate;
        private ComboBox cmbStatus;
        private Button btnSave, btnCancel;
        private Label lblOrderDetails, lblOrderDate, lblClient, lblTotal;
        
        private StockContext _context = new StockContext();

        public DeliveryEditForm() : this(new Delivery { DeliveryDate = DateTime.Now }) { }

        public DeliveryEditForm(Delivery delivery)
        {
            Delivery = delivery;
            InitializeComponent();
            LoadOrders();
            
            if (delivery.DeliveryId != 0)
            {
                cmbOrders.SelectedValue = delivery.OrderId;
                dtpDeliveryDate.Value = delivery.DeliveryDate;
                cmbStatus.Text = delivery.Status;
                cmbOrders.Enabled = false; // Can't change order after delivery is created
            }
        }

        private void InitializeComponent()
        {
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Text = "Delivery Details";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            
            Label lblOrder = new Label
            {
                Text = "Order:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            
            cmbOrders = new ComboBox
            {
                Location = new System.Drawing.Point(120, 20),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbOrders.SelectedIndexChanged += cmbOrders_SelectedIndexChanged;
            
            lblOrderDetails = new Label
            {
                Text = "Order Details:",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true,
                Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Bold)
            };
            
            lblOrderDate = new Label
            {
                Text = "Order Date: N/A",
                Location = new System.Drawing.Point(40, 85),
                AutoSize = true
            };
            
            lblClient = new Label
            {
                Text = "Client: N/A",
                Location = new System.Drawing.Point(40, 110),
                AutoSize = true
            };
            
            lblTotal = new Label
            {
                Text = "Total: N/A",
                Location = new System.Drawing.Point(40, 135),
                AutoSize = true
            };
            
            Label lblDeliveryDate = new Label
            {
                Text = "Delivery Date:",
                Location = new System.Drawing.Point(20, 170),
                AutoSize = true
            };
            
            dtpDeliveryDate = new DateTimePicker
            {
                Location = new System.Drawing.Point(120, 170),
                Width = 250,
                Format = DateTimePickerFormat.Short
            };
            
            Label lblStatus = new Label
            {
                Text = "Status:",
                Location = new System.Drawing.Point(20, 210),
                AutoSize = true
            };
            
            cmbStatus = new ComboBox
            {
                Location = new System.Drawing.Point(120, 210),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            
            // Add status options
            cmbStatus.Items.AddRange(new string[] { 
                "Pending", 
                "In Transit", 
                "Delivered", 
                "Canceled" 
            });
            cmbStatus.SelectedIndex = 0;
            
            btnSave = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(200, 250),
                Width = 80,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += btnSave_Click;
            
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(290, 250),
                Width = 80,
                DialogResult = DialogResult.Cancel
            };
            
            this.Controls.AddRange(new Control[] {
                lblOrder, cmbOrders,
                lblOrderDetails, lblOrderDate, lblClient, lblTotal,
                lblDeliveryDate, dtpDeliveryDate,
                lblStatus, cmbStatus,
                btnSave, btnCancel
            });
        }

        private void LoadOrders()
        {
            try
            {
                // Only show orders without deliveries or with the current delivery
                var orders = _context.Orders
                    .Where(o => !_context.Deliveries.Any(d => d.OrderId == o.OrderId) || 
                            (Delivery.DeliveryId != 0 && o.OrderId == Delivery.OrderId))
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new
                    {
                        o.OrderId,
                        DisplayText = $"Order #{o.OrderId} - {o.Client.Name} - {o.OrderDate:d}"
                    })
                    .ToList();
                
                cmbOrders.DisplayMember = "DisplayText";
                cmbOrders.ValueMember = "OrderId";
                cmbOrders.DataSource = orders;
                
                if (orders.Count == 0)
                {
                    MessageBox.Show("There are no orders available for delivery.", 
                        "No Orders", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbOrders_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbOrders.SelectedValue != null)
                {
                    int orderId = (int)cmbOrders.SelectedValue;
                    var order = _context.Orders.Find(orderId);
                    
                    if (order != null)
                    {
                        // Load order details
                        _context.Entry(order)
                            .Reference(o => o.Client)
                            .Load();
                            
                        lblOrderDate.Text = $"Order Date: {order.OrderDate:d}";
                        lblClient.Text = $"Client: {order.Client.Name}";
                        lblTotal.Text = $"Total: ${order.Total:F2}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbOrders.SelectedValue == null)
                {
                    MessageBox.Show("Please select an order.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }
                
                if (string.IsNullOrEmpty(cmbStatus.Text))
                {
                    MessageBox.Show("Please select a status.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }
                
                Delivery.OrderId = (int)cmbOrders.SelectedValue;
                Delivery.DeliveryDate = dtpDeliveryDate.Value;
                Delivery.Status = cmbStatus.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving delivery: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
