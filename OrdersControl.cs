using System;
using System.Linq;
using System.Windows.Forms;
using StockManagementApp.Models;
using System.Drawing.Printing;
using System.Drawing;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace StockManagementApp.Modules
{
    public partial class OrdersControl : UserControl
    {
        private StockContext _context = new StockContext();
        private PrintDocument printDocument = new PrintDocument();
        private Order currentPrintOrder;

        public OrdersControl()
        {
            InitializeComponent();
            LoadOrders();
            
            // Set up print document
            printDocument.PrintPage += PrintDocument_PrintPage;
        }        private void LoadOrders(string search = "")
        {
            try
            {
                var orders = _context.Orders
                    .Include(o => o.Client)
                    .Include(o => o.Items)
                    .Where(o => search == "" || o.Client.Name.Contains(search) || o.OrderDate.ToString().Contains(search))
                    .Select(o => new
                    {
                        o.OrderId,
                        o.OrderDate,
                        Client = o.Client.Name,
                        o.Total,
                        ItemCount = o.Items.Count
                    })
                    .ToList();
                dataGridViewOrders.DataSource = orders;
                
                // Format the grid for better readability
                if (dataGridViewOrders.Columns.Contains("Total"))
                {
                    dataGridViewOrders.Columns["Total"].DefaultCellStyle.Format = "C2";
                }
                
                if (dataGridViewOrders.Columns.Contains("OrderDate"))
                {
                    dataGridViewOrders.Columns["OrderDate"].DefaultCellStyle.Format = "d";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var form = new OrderEditForm();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _context.Orders.Add(form.Order);
                    _context.SaveChanges();
                    LoadOrders();
                    
                    // Log the action in history
                    LogAction($"Added order #{form.Order.OrderId} for client: {form.Order.Client.Name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewOrders.CurrentRow != null)
                {
                    int id = (int)dataGridViewOrders.CurrentRow.Cells["OrderId"].Value;
                    var order = _context.Orders.Find(id);
                    
                    if (order != null)
                    {
                        // Load order items
                        _context.Entry(order)
                            .Collection(o => o.Items)
                            .Load();
                            
                        // Load client
                        _context.Entry(order)
                            .Reference(o => o.Client)
                            .Load();
                            
                        var form = new OrderEditForm(order);
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            _context.SaveChanges();
                            LoadOrders();
                            
                            // Log the action in history
                            LogAction($"Updated order #{order.OrderId} for client: {order.Client.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewOrders.CurrentRow != null)
                {
                    int id = (int)dataGridViewOrders.CurrentRow.Cells["OrderId"].Value;
                    var order = _context.Orders.Find(id);
                    
                    if (order != null)
                    {
                        var result = MessageBox.Show($"Are you sure you want to delete order #{order.OrderId}?", 
                            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            
                        if (result == DialogResult.Yes)
                        {
                            // Load order items
                            _context.Entry(order)
                                .Collection(o => o.Items)
                                .Load();
                                
                            // Return items to inventory
                            foreach (var item in order.Items)
                            {
                                var product = _context.Products.Find(item.ProductId);
                                if (product != null)
                                {
                                    product.Quantity += item.Quantity;
                                }
                            }
                              // Remove order items
                            foreach (var item in order.Items.ToList())
                            {
                                _context.OrderItems.Remove(item);
                            }
                            
                            // Remove the order
                            _context.Orders.Remove(order);
                            _context.SaveChanges();
                            LoadOrders();
                            
                            // Log the action in history
                            LogAction($"Deleted order #{id}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadOrders(txtSearch.Text);
        }
        
        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridViewOrders.CurrentRow != null)
                {
                    int id = (int)dataGridViewOrders.CurrentRow.Cells["OrderId"].Value;
                    var order = _context.Orders.Find(id);
                    
                    if (order != null)
                    {
                        // Load order items and references
                        _context.Entry(order)
                            .Collection(o => o.Items)
                            .Load();
                            
                        _context.Entry(order)
                            .Reference(o => o.Client)
                            .Load();
                            
                        foreach (var item in order.Items)
                        {
                            _context.Entry(item)
                                .Reference(i => i.Product)
                                .Load();
                        }
                        
                        currentPrintOrder = order;
                        
                        // Show print preview or print directly
                        var result = MessageBox.Show("Do you want to preview the invoice before printing?", 
                            "Print Options", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                            
                        if (result == DialogResult.Yes)
                        {
                            PrintPreviewDialog preview = new PrintPreviewDialog();
                            preview.Document = printDocument;
                            preview.ShowDialog();
                        }
                        else if (result == DialogResult.No)
                        {
                            printDocument.Print();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                if (currentPrintOrder != null)
                {
                    // Set up fonts
                    Font titleFont = new Font("Arial", 16, FontStyle.Bold);
                    Font headerFont = new Font("Arial", 12, FontStyle.Bold);
                    Font normalFont = new Font("Arial", 10);
                    Font totalFont = new Font("Arial", 12, FontStyle.Bold);
                    
                    // Set up measurements
                    int startX = 50;
                    int startY = 50;
                    int offset = 20;
                    int currentY = startY;
                    
                    // Print header
                    e.Graphics.DrawString("INVOICE", titleFont, Brushes.Black, startX, currentY);
                    currentY += offset * 2;
                    
                    e.Graphics.DrawString($"Invoice #: {currentPrintOrder.OrderId}", headerFont, Brushes.Black, startX, currentY);
                    currentY += offset;
                    
                    e.Graphics.DrawString($"Date: {currentPrintOrder.OrderDate:d}", headerFont, Brushes.Black, startX, currentY);
                    currentY += offset;
                    
                    e.Graphics.DrawString($"Client: {currentPrintOrder.Client.Name}", headerFont, Brushes.Black, startX, currentY);
                    currentY += offset;
                    
                    e.Graphics.DrawString($"Address: {currentPrintOrder.Client.Address}", normalFont, Brushes.Black, startX, currentY);
                    currentY += offset;
                    
                    e.Graphics.DrawString($"Phone: {currentPrintOrder.Client.Phone}", normalFont, Brushes.Black, startX, currentY);
                    currentY += offset * 2;
                    
                    // Print table header
                    e.Graphics.DrawString("Product", headerFont, Brushes.Black, startX, currentY);
                    e.Graphics.DrawString("Quantity", headerFont, Brushes.Black, startX + 250, currentY);
                    e.Graphics.DrawString("Price", headerFont, Brushes.Black, startX + 350, currentY);
                    e.Graphics.DrawString("Subtotal", headerFont, Brushes.Black, startX + 450, currentY);
                    currentY += offset;
                    
                    // Draw line under header
                    e.Graphics.DrawLine(Pens.Black, startX, currentY, startX + 550, currentY);
                    currentY += 5;
                    
                    // Print items
                    foreach (var item in currentPrintOrder.Items)
                    {
                        currentY += offset;
                        e.Graphics.DrawString(item.Product.Name, normalFont, Brushes.Black, startX, currentY);
                        e.Graphics.DrawString(item.Quantity.ToString(), normalFont, Brushes.Black, startX + 250, currentY);
                        e.Graphics.DrawString($"${item.Price:F2}", normalFont, Brushes.Black, startX + 350, currentY);
                        e.Graphics.DrawString($"${item.Quantity * item.Price:F2}", normalFont, Brushes.Black, startX + 450, currentY);
                    }
                    
                    // Draw line under items
                    currentY += offset;
                    e.Graphics.DrawLine(Pens.Black, startX, currentY, startX + 550, currentY);
                    currentY += offset;
                    
                    // Print total
                    e.Graphics.DrawString($"Total: ${currentPrintOrder.Total:F2}", totalFont, Brushes.Black, startX + 350, currentY);
                    
                    // Print footer
                    currentY += offset * 3;
                    e.Graphics.DrawString("Thank you for your business!", normalFont, Brushes.Black, startX, currentY);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating print output: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStats_Click(object sender, EventArgs e)
        {
            try
            {
                // Calculate statistics
                var totalOrders = _context.Orders.Count();
                var totalSales = _context.Orders.Sum(o => o.Total);
                var avgOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
                
                // Get orders first to avoid translation errors
                var orders = _context.Orders.Select(o => new { o.OrderDate, o.Total }).ToList();
                
                // Now perform grouping in memory
                var monthlySales = orders
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new 
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Total = g.Sum(o => o.Total)
                    })
                    .OrderBy(x => x.Month)
                    .ToList();
                
                // Format the statistics
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Order Statistics");
                sb.AppendLine("============================");
                sb.AppendLine($"Total Orders: {totalOrders}");
                sb.AppendLine($"Total Sales: ${totalSales:F2}");
                sb.AppendLine($"Average Order Value: ${avgOrderValue:F2}");
                sb.AppendLine();
                sb.AppendLine("Monthly Sales:");
                sb.AppendLine("============================");
                
                foreach (var item in monthlySales)
                {
                    sb.AppendLine($"{item.Month}: ${item.Total:F2}");
                }
                
                // Display in a message box for now
                // In a real app, you'd show this in a proper dashboard or report form
                MessageBox.Show(sb.ToString(), "Order Statistics", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating statistics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
