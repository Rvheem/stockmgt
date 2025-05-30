using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms; // Use this Timer explicitly
using StockManagementApp.Models;

namespace StockManagementApp.Modules
{
    public class Dashboard : UserControl
    {
        private Panel panelProducts;
        private Panel panelOrders;
        private Panel panelClients;
        private Panel panelSuppliers;
        private Panel panelAlerts;
        private Label lblProductsTitle;
        private Label lblProductsCount;
        private Label lblOrdersTitle;
        private Label lblOrdersCount;
        private Label lblClientsTitle;
        private Label lblClientsCount;
        private Label lblSuppliersTitle;
        private Label lblSuppliersCount;
        private Label lblAlertsTitle;
        private ListBox listAlerts;
        private System.Windows.Forms.Timer refreshTimer = new System.Windows.Forms.Timer(); // Replace the ambiguous Timer with explicit reference

        public Dashboard()
        {
            InitializeComponent();
            LoadDashboardData();
            
            // Set up a timer to refresh the dashboard every 5 minutes
            refreshTimer.Interval = 300000; // 5 minutes in milliseconds
            refreshTimer.Tick += (sender, e) => LoadDashboardData();
            refreshTimer.Start();
        }

        private void InitializeComponent()
        {
            this.panelProducts = new Panel();
            this.lblProductsTitle = new Label();
            this.lblProductsCount = new Label();
            
            this.panelOrders = new Panel();
            this.lblOrdersTitle = new Label();
            this.lblOrdersCount = new Label();
            
            this.panelClients = new Panel();
            this.lblClientsTitle = new Label();
            this.lblClientsCount = new Label();
            
            this.panelSuppliers = new Panel();
            this.lblSuppliersTitle = new Label();
            this.lblSuppliersCount = new Label();
            
            this.panelAlerts = new Panel();
            this.lblAlertsTitle = new Label();
            this.listAlerts = new ListBox();
            
            // Panel Products
            this.panelProducts.BorderStyle = BorderStyle.FixedSingle;
            this.panelProducts.Location = new Point(20, 20);
            this.panelProducts.Size = new Size(230, 100);
            this.panelProducts.BackColor = Color.FromArgb(41, 128, 185);
            
            this.lblProductsTitle.AutoSize = true;
            this.lblProductsTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.lblProductsTitle.ForeColor = Color.White;
            this.lblProductsTitle.Location = new Point(10, 10);
            this.lblProductsTitle.Text = "Products";
            
            this.lblProductsCount.AutoSize = true;
            this.lblProductsCount.Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Bold);
            this.lblProductsCount.ForeColor = Color.White;
            this.lblProductsCount.Location = new Point(10, 40);
            this.lblProductsCount.Text = "0";
            
            this.panelProducts.Controls.Add(this.lblProductsTitle);
            this.panelProducts.Controls.Add(this.lblProductsCount);
            
            // Panel Orders
            this.panelOrders.BorderStyle = BorderStyle.FixedSingle;
            this.panelOrders.Location = new Point(270, 20);
            this.panelOrders.Size = new Size(230, 100);
            this.panelOrders.BackColor = Color.FromArgb(39, 174, 96);
            
            this.lblOrdersTitle.AutoSize = true;
            this.lblOrdersTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.lblOrdersTitle.ForeColor = Color.White;
            this.lblOrdersTitle.Location = new Point(10, 10);
            this.lblOrdersTitle.Text = "Orders";
            
            this.lblOrdersCount.AutoSize = true;
            this.lblOrdersCount.Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Bold);
            this.lblOrdersCount.ForeColor = Color.White;
            this.lblOrdersCount.Location = new Point(10, 40);
            this.lblOrdersCount.Text = "0";
            
            this.panelOrders.Controls.Add(this.lblOrdersTitle);
            this.panelOrders.Controls.Add(this.lblOrdersCount);
            
            // Panel Clients
            this.panelClients.BorderStyle = BorderStyle.FixedSingle;
            this.panelClients.Location = new Point(520, 20);
            this.panelClients.Size = new Size(230, 100);
            this.panelClients.BackColor = Color.FromArgb(192, 57, 43);
            
            this.lblClientsTitle.AutoSize = true;
            this.lblClientsTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.lblClientsTitle.ForeColor = Color.White;
            this.lblClientsTitle.Location = new Point(10, 10);
            this.lblClientsTitle.Text = "Clients";
            
            this.lblClientsCount.AutoSize = true;
            this.lblClientsCount.Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Bold);
            this.lblClientsCount.ForeColor = Color.White;
            this.lblClientsCount.Location = new Point(10, 40);
            this.lblClientsCount.Text = "0";
            
            this.panelClients.Controls.Add(this.lblClientsTitle);
            this.panelClients.Controls.Add(this.lblClientsCount);
            
            // Panel Suppliers
            this.panelSuppliers.BorderStyle = BorderStyle.FixedSingle;
            this.panelSuppliers.Location = new Point(20, 140);
            this.panelSuppliers.Size = new Size(230, 100);
            this.panelSuppliers.BackColor = Color.FromArgb(142, 68, 173);
            
            this.lblSuppliersTitle.AutoSize = true;
            this.lblSuppliersTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.lblSuppliersTitle.ForeColor = Color.White;
            this.lblSuppliersTitle.Location = new Point(10, 10);
            this.lblSuppliersTitle.Text = "Suppliers";
            
            this.lblSuppliersCount.AutoSize = true;
            this.lblSuppliersCount.Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Bold);
            this.lblSuppliersCount.ForeColor = Color.White;
            this.lblSuppliersCount.Location = new Point(10, 40);
            this.lblSuppliersCount.Text = "0";
            
            this.panelSuppliers.Controls.Add(this.lblSuppliersTitle);
            this.panelSuppliers.Controls.Add(this.lblSuppliersCount);
            
            // Panel Alerts
            this.panelAlerts.BorderStyle = BorderStyle.FixedSingle;
            this.panelAlerts.Location = new Point(20, 260);
            this.panelAlerts.Size = new Size(730, 280);
            this.panelAlerts.BackColor = Color.White;
            
            this.lblAlertsTitle.AutoSize = true;
            this.lblAlertsTitle.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            this.lblAlertsTitle.ForeColor = Color.FromArgb(192, 57, 43);
            this.lblAlertsTitle.Location = new Point(10, 10);
            this.lblAlertsTitle.Text = "Stock Alerts";
            
            this.listAlerts.Location = new Point(10, 40);
            this.listAlerts.Size = new Size(710, 230);
            this.listAlerts.Font = new Font("Microsoft Sans Serif", 10F);
            this.listAlerts.ForeColor = Color.FromArgb(192, 57, 43);
            
            this.panelAlerts.Controls.Add(this.lblAlertsTitle);
            this.panelAlerts.Controls.Add(this.listAlerts);
            
            // Add all panels to the control
            this.Controls.Add(this.panelProducts);
            this.Controls.Add(this.panelOrders);
            this.Controls.Add(this.panelClients);
            this.Controls.Add(this.panelSuppliers);
            this.Controls.Add(this.panelAlerts);
            
            this.Size = new Size(780, 570);
        }

        private void LoadDashboardData()
        {
            try
            {
                using (var context = new StockContext())
                {
                    // Load counts
                    lblProductsCount.Text = context.Products.Count().ToString();
                    lblOrdersCount.Text = context.Orders.Count().ToString();
                    lblClientsCount.Text = context.Clients.Count().ToString();
                    lblSuppliersCount.Text = context.Suppliers.Count().ToString();
                    
                    // Load alerts
                    listAlerts.Items.Clear();
                    
                    // Low stock alerts
                    var lowStockProducts = context.Products
                        .Where(p => p.Quantity <= p.Threshold)
                        .OrderBy(p => p.Quantity)
                        .ToList();
                        
                    foreach (var product in lowStockProducts)
                    {
                        listAlerts.Items.Add($"LOW STOCK: {product.Name} (Qty: {product.Quantity}, Threshold: {product.Threshold})");
                    }
                    
                    // Expiry alerts
                    var expiryProducts = context.Products
                        .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate <= DateTime.Now.AddDays(7))
                        .OrderBy(p => p.ExpiryDate)
                        .ToList();
                        
                    foreach (var product in expiryProducts)
                    {
                        if (product.ExpiryDate <= DateTime.Now)
                        {
                            listAlerts.Items.Add($"EXPIRED: {product.Name} expired on {product.ExpiryDate.Value.ToShortDateString()}");
                        }
                        else
                        {
                            listAlerts.Items.Add($"EXPIRING SOON: {product.Name} expires on {product.ExpiryDate.Value.ToShortDateString()}");
                        }
                    }
                    
                    if (listAlerts.Items.Count == 0)
                    {
                        listAlerts.Items.Add("No alerts at this time.");
                    }
                }
            }
            catch (Exception ex)
            {
                listAlerts.Items.Clear();
                listAlerts.Items.Add($"Error loading data: {ex.Message}");
            }
        }
    }
}
