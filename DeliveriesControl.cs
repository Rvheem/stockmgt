using System;
using System.Linq;
using System.Windows.Forms;
using StockManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace StockManagementApp.Modules
{
    public partial class DeliveriesControl : UserControl
    {
        private StockContext _context;

        public DeliveriesControl()
        {
            InitializeComponent();
            _context = DbContextFactory.CreateContext();
            LoadDeliveries();
        }

        private void LoadDeliveries(string search = "")
        {
            try
            {
                var deliveries = _context.Deliveries
                    .Where(d => search == "" || d.Status.Contains(search) || 
                          d.Order.Client.Name.Contains(search))
                    .Select(d => new
                    {
                        d.DeliveryId,
                        d.DeliveryDate,
                        d.Status,
                        OrderId = d.OrderId,
                        ClientName = d.Order.Client.Name
                    })
                    .ToList();
                dataGridViewDeliveries.DataSource = deliveries;
                
                // Format grid for better readability
                if (dataGridViewDeliveries.Columns.Contains("DeliveryId"))
                {
                    dataGridViewDeliveries.Columns["DeliveryId"].Visible = false;
                }
                
                if (dataGridViewDeliveries.Columns.Contains("DeliveryDate"))
                {
                    dataGridViewDeliveries.Columns["DeliveryDate"].DefaultCellStyle.Format = "d";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading deliveries: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);            }
        }
        
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var form = new DeliveryEditForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _context.Deliveries.Add(form.Delivery);
                    _context.SaveChanges();
                    LoadDeliveries();
                    
                    // Log the action in history
                    LogAction($"Added delivery for order #{form.Delivery.OrderId}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding delivery: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewDeliveries.CurrentRow != null)
                {
                    int id = (int)dataGridViewDeliveries.CurrentRow.Cells["DeliveryId"].Value;
                    var delivery = _context.Deliveries.Find(id);
                    
                    if (delivery != null)
                    {
                        var form = new DeliveryEditForm(delivery);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            _context.SaveChanges();
                            LoadDeliveries();
                            
                            // Log the action in history
                            LogAction($"Updated delivery #{delivery.DeliveryId} for order #{delivery.OrderId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing delivery: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewDeliveries.CurrentRow != null)
                {
                    int id = (int)dataGridViewDeliveries.CurrentRow.Cells["DeliveryId"].Value;
                    var delivery = _context.Deliveries.Find(id);
                    
                    if (delivery != null)
                    {
                        var result = MessageBox.Show($"Are you sure you want to delete delivery #{delivery.DeliveryId}?", 
                            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            
                        if (result == DialogResult.Yes)
                        {
                            _context.Deliveries.Remove(delivery);
                            _context.SaveChanges();
                            LoadDeliveries();
                            
                            // Log the action in history
                            LogAction($"Deleted delivery #{delivery.DeliveryId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting delivery: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadDeliveries(txtSearch.Text);
        }
        
        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewDeliveries.CurrentRow != null)
                {
                    int id = (int)dataGridViewDeliveries.CurrentRow.Cells["DeliveryId"].Value;
                    var delivery = _context.Deliveries.Find(id);
                    
                    if (delivery != null)
                    {
                        // Show a dialog to select the new status
                        using (var form = new Form())
                        {
                            form.Text = "Update Delivery Status";
                            form.Size = new System.Drawing.Size(300, 150);
                            form.FormBorderStyle = FormBorderStyle.FixedDialog;
                            form.StartPosition = FormStartPosition.CenterParent;
                            form.MaximizeBox = false;
                            form.MinimizeBox = false;
                            
                            var lblStatus = new Label
                            {
                                Text = "Status:",
                                Location = new System.Drawing.Point(20, 20),
                                AutoSize = true
                            };
                            
                            var cmbStatus = new ComboBox
                            {
                                Location = new System.Drawing.Point(80, 20),
                                Width = 180,
                                DropDownStyle = ComboBoxStyle.DropDownList
                            };
                            
                            // Add status options
                            cmbStatus.Items.AddRange(new string[] { 
                                "Pending", 
                                "In Transit", 
                                "Delivered", 
                                "Canceled" 
                            });
                            cmbStatus.Text = delivery.Status;
                            
                            var btnOk = new Button
                            {
                                Text = "OK",
                                DialogResult = DialogResult.OK,
                                Location = new System.Drawing.Point(100, 70),
                                Width = 75
                            };
                            
                            var btnCancel = new Button
                            {
                                Text = "Cancel",
                                DialogResult = DialogResult.Cancel,
                                Location = new System.Drawing.Point(190, 70),
                                Width = 75
                            };
                            
                            form.Controls.AddRange(new Control[] { lblStatus, cmbStatus, btnOk, btnCancel });
                            form.AcceptButton = btnOk;
                            form.CancelButton = btnCancel;
                            
                            if (form.ShowDialog() == DialogResult.OK)
                            {
                                string oldStatus = delivery.Status;
                                delivery.Status = cmbStatus.Text;
                                _context.SaveChanges();
                                LoadDeliveries();
                                
                                // Log the action in history
                                LogAction($"Updated delivery #{delivery.DeliveryId} status from '{oldStatus}' to '{delivery.Status}'");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnDetails_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewDeliveries.CurrentRow != null)
                {
                    int id = (int)dataGridViewDeliveries.CurrentRow.Cells["DeliveryId"].Value;
                    var delivery = _context.Deliveries
                        .Include(d => d.Order)
                        .ThenInclude(o => o.Client)
                        .Include(d => d.Order)
                        .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                        .FirstOrDefault(d => d.DeliveryId == id);
                    
                    if (delivery != null)
                    {
                        // Show delivery details in a form or dialog
                        MessageBox.Show(
                            $"Delivery #{delivery.DeliveryId}\n" +
                            $"Date: {delivery.DeliveryDate:d}\n" +
                            $"Status: {delivery.Status}\n" +
                            $"Order #: {delivery.OrderId}\n" +
                            $"Client: {delivery.Order.Client.Name}\n" +
                            $"Items: {delivery.Order.OrderItems.Count}", 
                            "Delivery Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing delivery details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            {
                // Just log to console for now, don't disrupt the UI
                System.Diagnostics.Debug.WriteLine($"Error logging action: {ex.Message}");
            }
        }
    }
}