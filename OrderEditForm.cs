using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using StockManagementApp.Models;

namespace StockManagementApp.Modules
{
    public partial class OrderEditForm : Form
    {
        private StockContext _context;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Order? OrderToEdit { get; set; } = null;
        private bool _isNewOrder;
        private List<OrderItem> _orderItems = new List<OrderItem>();
        private BindingSource _bindingSource = new BindingSource();
        public Order Order => OrderToEdit ?? new Order { OrderDate = DateTime.Now };
        
        public OrderEditForm()
        {
            InitializeComponent();
            _context = DbContextFactory.CreateContext();
            _isNewOrder = true;
            OrderToEdit = new Order { OrderDate = DateTime.Now };
            InitializeOrderForm();
        }

        public OrderEditForm(Order? order)
        {
            InitializeComponent();
            _context = DbContextFactory.CreateContext();
            OrderToEdit = order ?? new Order { OrderDate = DateTime.Now };
            _isNewOrder = order == null;
            InitializeOrderForm();
        }
        
        private void InitializeOrderForm()
        {
            // Load clients for the dropdown
            LoadClients();
            
            // Load products for the dropdown
            LoadProducts();
            
            // Initialize the order items grid
            InitializeDataGrid();
            
            if (!_isNewOrder && OrderToEdit != null)
            {
                // Load existing order data
                dtpOrderDate.Value = OrderToEdit.OrderDate;
                cmbClient.SelectedValue = OrderToEdit.ClientId;
                
                // Load order items
                LoadOrderItems();
                
                CalculateTotal();
            }
        }
        
        private void LoadClients()
        {
            try
            {
                var clients = _context.Clients
                    .OrderBy(c => c.Name)
                    .Select(c => new {
                        c.ClientId,
                        DisplayName = c.Name + (string.IsNullOrEmpty(c.Phone) ? "" : $" ({c.Phone})")
                    })
                    .ToList();
                cmbClient.DataSource = clients;
                cmbClient.DisplayMember = "DisplayName";
                cmbClient.ValueMember = "ClientId";
                cmbClient.SelectedIndex = -1;

                if (clients.Count > 0) cmbClient.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading clients: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadProducts()
        {
            try
            {
                var products = _context.Products
                    .Where(p => p.Quantity > 0)
                    .Select(p => new { 
                        p.ProductId, 
                        DisplayName = $"{p.Name} ({p.Quantity} in stock, ${p.Price:F2})",
                        p.Price,
                        p.Quantity
                    })
                    .ToList();
                cmbProduct.DataSource = products;
                cmbProduct.DisplayMember = "DisplayName";
                cmbProduct.ValueMember = "ProductId";
                cmbProduct.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbProduct.SelectedValue == null)
                {
                    MessageBox.Show("Please select a product.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                int productId = (int)cmbProduct.SelectedValue;
                int quantity = (int)numQuantity.Value;
                
                if (quantity <= 0)
                {
                    MessageBox.Show("Quantity must be greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Get the product to check availability and get price
                var product = _context.Products.Find(productId);
                
                if (product == null)
                {
                    MessageBox.Show("Selected product not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Check if we already have this product in the order
                var existingItem = _orderItems.FirstOrDefault(i => i.ProductId == productId);
                
                if (existingItem != null)
                {
                    // Update the existing item
                    int totalQuantity = existingItem.Quantity + quantity;
                    
                    if (totalQuantity > product.Quantity)
                    {
                        MessageBox.Show($"Cannot add {quantity} more units. Only {product.Quantity} available in stock.", 
                            "Insufficient Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    existingItem.Quantity = totalQuantity;
                }
                else
                {
                    // Add a new item
                    if (quantity > product.Quantity)
                    {
                        MessageBox.Show($"Cannot add {quantity} units. Only {product.Quantity} available in stock.", 
                            "Insufficient Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    var newItem = new OrderItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        Price = (decimal)product.Price
                    };
                    
                    _orderItems.Add(newItem);
                }
                
                // Refresh the grid
                _bindingSource.ResetBindings(false);
                
                // Update the total
                UpdateTotal();
                
                // Reset the inputs
                cmbProduct.SelectedIndex = -1;
                numQuantity.Value = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRemoveItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvOrderItems.CurrentRow != null)
                {
                    int index = dgvOrderItems.CurrentRow.Index;
                    _orderItems.RemoveAt(index);
                    
                    // Refresh the grid
                    _bindingSource.ResetBindings(false);
                    
                    // Update the total
                    UpdateTotal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            
            foreach (var item in _orderItems)
            {
                total += item.Price * item.Quantity;
            }
            
            txtTotal.Text = $"${total:F2}";
            OrderToEdit.Total = (decimal)total;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate form inputs
                if (cmbClient.SelectedValue == null)
                {
                    MessageBox.Show("Please select a client.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbClient.Focus();
                    return;
                }
                
                if (_orderItems.Count == 0)
                {
                    MessageBox.Show("Please add at least one item to the order.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update the order object with form values
                OrderToEdit.ClientId = (int)cmbClient.SelectedValue;
                OrderToEdit.OrderDate = dtpOrderDate.Value;
                
                // If this is a new order, add the items
                if (_isNewOrder)
                {
                    OrderToEdit.Items = _orderItems;
                }
                // If editing, update the items collection
                else
                {
                    // Remove items that are no longer in the order
                    var itemsToRemove = OrderToEdit.Items.Where(i => !_orderItems.Any(oi => oi.OrderItemId == i.OrderItemId)).ToList();
                    foreach (var item in itemsToRemove)
                    {
                        OrderToEdit.Items.Remove(item);
                    }
                    
                    // Update existing items and add new ones
                    foreach (var item in _orderItems)
                    {
                        if (item.OrderItemId == 0)
                        {
                            // This is a new item, add it
                            OrderToEdit.Items.Add(item);
                        }
                        else
                        {
                            // This is an existing item, update it
                            var existingItem = OrderToEdit.Items.FirstOrDefault(i => i.OrderItemId == item.OrderItemId);
                            if (existingItem != null)
                            {
                                existingItem.Quantity = item.Quantity;
                                existingItem.Price = item.Price;
                            }
                        }
                    }
                }
                
                // Deduct the items from inventory
                foreach (var item in _orderItems)
                {
                    var product = _context.Products.Find(item.ProductId);
                    if (product != null)
                    {
                        // If this is a new order, deduct the entire quantity
                        if (_isNewOrder)
                        {
                            product.Quantity -= item.Quantity;
                        }
                        // If editing, only deduct the difference
                        else
                        {
                            var originalItem = OrderToEdit.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                            if (originalItem != null)
                            {
                                int difference = item.Quantity - originalItem.Quantity;
                                if (difference != 0)
                                {
                                    product.Quantity -= difference;
                                }
                            }
                            else
                            {
                                // This is a new item in an existing order
                                product.Quantity -= item.Quantity;
                            }
                        }
                    }
                }

                // Try to save changes and catch inner exception details
                try
                {
                    _context.SaveChanges();
                    DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message;
                    Exception? inner = ex.InnerException;
                    int depth = 0;
                    while (inner != null && depth < 5)
                    {
                        errorMsg += $"\nInner Exception [{depth + 1}]: {inner.Message}";
                        inner = inner.InnerException;
                        depth++;
                    }
                    errorMsg += $"\nStack Trace:\n{ex.StackTrace}";
                    MessageBox.Show($"Error saving order (DB): {errorMsg}", "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Prevent closing the dialog if there is an error
                    this.DialogResult = DialogResult.None;
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
        
        private void InitializeDataGrid()
        {
            // Set up the data grid for order items
            dgvOrderItems.AutoGenerateColumns = false;
            
            // Add columns if they don't exist already
            if (dgvOrderItems.Columns.Count == 0)
            {
                dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "ProductId",
                    HeaderText = "Product ID",
                    Visible = false
                });
                
                dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "ProductName",
                    HeaderText = "Product",
                    Width = 200,
                    ReadOnly = true
                });
                
                dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Quantity",
                    HeaderText = "Quantity",
                    Width = 80
                });
                
                dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "UnitPrice",
                    HeaderText = "Unit Price",
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
                });
                
                dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Subtotal",
                    HeaderText = "Subtotal",
                    Width = 100,
                    ReadOnly = true,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
                });
                
                var deleteColumn = new DataGridViewButtonColumn
                {
                    HeaderText = "",
                    Text = "Remove",
                    UseColumnTextForButtonValue = true,
                    Width = 80
                };
                dgvOrderItems.Columns.Add(deleteColumn);
            }
            
            // Set up binding source
            _bindingSource.DataSource = _orderItems;
            dgvOrderItems.DataSource = _bindingSource;
        }
        
        private void LoadOrderItems()
        {
            try
            {
                // Clear existing items
                _orderItems.Clear();
                
                // If editing an existing order, load its items
                if (!_isNewOrder && OrderToEdit != null)
                {
                    var orderItems = _context.OrderItems
                        .Where(oi => oi.OrderId == OrderToEdit.OrderId)
                        .ToList();
                        
                    foreach (var item in orderItems)
                    {
                        // Load product separately since Include() isn't working here
                        var product = _context.Products.Find(item.ProductId);
                        
                        _orderItems.Add(new OrderItem
                        {
                            OrderItemId = item.OrderItemId,
                            OrderId = item.OrderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            Product = product
                        });
                    }
                }
                
                // Refresh binding
                _bindingSource.ResetBindings(false);
                CalculateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order items: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void CalculateTotal()
        {
            decimal total = 0;
            
            foreach (var item in _orderItems)
            {
                total += item.Quantity * item.Price;
            }
            
            // Update the total label
            lblTotal.Text = $"Total: ${total:F2}";
            
            // Update the order total
            if (OrderToEdit != null)
            {
                OrderToEdit.Total = total;
            }
        }
    }
}
