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
        private StockContext _context = new StockContext();
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Order Order { get; private set; }
        private bool _isNewOrder;
        private List<OrderItem> _orderItems = new List<OrderItem>();
        private BindingSource _bindingSource = new BindingSource();

        public OrderEditForm(Order order = null)
        {
            InitializeComponent();
            
            // Initialize with a new order if null was passed
            Order = order ?? new Order();
            
            // Load clients for the dropdown
            LoadClients();
            
            // Load products for the dropdown
            LoadProducts();
            
            if (order == null)
            {
                // Create new order
                Order = new Order
                {
                    OrderDate = DateTime.Now,
                    Items = new List<OrderItem>()
                };
                _isNewOrder = true;
                this.Text = "Add New Order";
            }
            else
            {
                // Edit existing order
                Order = order;
                _isNewOrder = false;
                this.Text = "Edit Order";
                
                // Load order items
                _orderItems = Order.Items.ToList();
            }

            // Setup the items DataGridView
            _bindingSource.DataSource = _orderItems;
            dgvOrderItems.DataSource = _bindingSource;
            
            // Populate the form with order data if editing
            if (!_isNewOrder)
            {
                cmbClient.SelectedValue = Order.ClientId;
                dtpOrderDate.Value = Order.OrderDate;
                
                // Update the total
                UpdateTotal();
            }
        }
        
        private void LoadClients()
        {
            try
            {
                var clients = _context.Clients.ToList();
                cmbClient.DataSource = clients;
                cmbClient.DisplayMember = "Name";
                cmbClient.ValueMember = "ClientId";
                cmbClient.SelectedIndex = -1;
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
                        Product = product,
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
            Order.Total = (decimal)total;
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
                Order.ClientId = (int)cmbClient.SelectedValue;
                Order.OrderDate = dtpOrderDate.Value;
                
                // If this is a new order, add the items
                if (_isNewOrder)
                {
                    Order.Items = _orderItems;
                }
                // If editing, update the items collection
                else
                {
                    // Remove items that are no longer in the order
                    var itemsToRemove = Order.Items.Where(i => !_orderItems.Any(oi => oi.OrderItemId == i.OrderItemId)).ToList();
                    foreach (var item in itemsToRemove)
                    {
                        Order.Items.Remove(item);
                    }
                    
                    // Update existing items and add new ones
                    foreach (var item in _orderItems)
                    {
                        if (item.OrderItemId == 0)
                        {
                            // This is a new item, add it
                            Order.Items.Add(item);
                        }
                        else
                        {
                            // This is an existing item, update it
                            var existingItem = Order.Items.FirstOrDefault(i => i.OrderItemId == item.OrderItemId);
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
                            var originalItem = Order.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
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

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
