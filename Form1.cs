namespace StockManagementApp;

using System.ComponentModel;
using StockManagementApp.Models;
using StockManagementApp.Modules;

public partial class Form1 : Form
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public User? CurrentUser { get; set; }
    
    // Initialize with empty implementations to avoid null warnings
    private ProductsControl productsControl = new();
    private OrdersControl ordersControl = new();
    private ClientsControl clientsControl = new();
    private SuppliersControl suppliersControl = new();
    private DeliveriesControl deliveriesControl = new();
    private UsersControl usersControl = new();
    private HistoryControl historyControl = new();
    private Dashboard dashboardControl = new();

    public Form1()
    {
        InitializeComponent();
        ShowLoginForm();
    }

    public Form1(User user)
    {
        InitializeComponent();
        CurrentUser = user;
        InitializeModules();
        ShowModule("Dashboard");
        this.Text = $"Stock Management System - Logged in as: {user.Username} ({user.Role})";
    }

    private void ShowLoginForm()
    {
        var loginForm = new LoginForm();
        if (loginForm.ShowDialog() == DialogResult.OK)
        {
            CurrentUser = loginForm.User;
            InitializeModules();
            ShowModule("Dashboard");
            this.Text = $"Stock Management System - Logged in as: {CurrentUser.Username} ({CurrentUser.Role})";
        }
        else
        {
            Application.Exit();
        }
    }

    private void InitializeModules()
    {
        dashboardControl = new Dashboard();
        productsControl = new ProductsControl();
        ordersControl = new OrdersControl();
        clientsControl = new ClientsControl();
        suppliersControl = new SuppliersControl();
        deliveriesControl = new DeliveriesControl();
        usersControl = new UsersControl();
        historyControl = new HistoryControl();
        
        // Initialize all controls but don't show them yet
        dashboardControl.Dock = DockStyle.Fill;
        productsControl.Dock = DockStyle.Fill;
        ordersControl.Dock = DockStyle.Fill;
        clientsControl.Dock = DockStyle.Fill;
        suppliersControl.Dock = DockStyle.Fill;
        deliveriesControl.Dock = DockStyle.Fill;
        usersControl.Dock = DockStyle.Fill;
        historyControl.Dock = DockStyle.Fill;
        
        panelContent.Controls.Add(dashboardControl);
        panelContent.Controls.Add(productsControl);
        panelContent.Controls.Add(ordersControl);
        panelContent.Controls.Add(clientsControl);
        panelContent.Controls.Add(suppliersControl);
        panelContent.Controls.Add(deliveriesControl);
        panelContent.Controls.Add(usersControl);
        panelContent.Controls.Add(historyControl);
        
        // Hide all modules initially
        foreach (Control control in panelContent.Controls)
        {
            control.Visible = false;
        }
        
        // Set up menu access based on user role
        if (CurrentUser.Role != "Administrator")
        {
            btnUsers.Visible = false;
        }
    }

    private void ShowModule(string moduleName)
    {
        // Hide all controls first
        foreach (Control control in panelContent.Controls)
        {
            control.Visible = false;
        }
        
        // Show the selected control
        switch (moduleName)
        {
            case "Dashboard":
                dashboardControl.Visible = true;
                break;
            case "Products":
                productsControl.Visible = true;
                break;
            case "Orders":
                ordersControl.Visible = true;
                break;
            case "Clients":
                clientsControl.Visible = true;
                break;
            case "Suppliers":
                suppliersControl.Visible = true;
                break;
            case "Deliveries":
                deliveriesControl.Visible = true;
                break;
            case "Users":
                usersControl.Visible = true;
                break;
            case "History":
                historyControl.Visible = true;
                break;
        }
        
        // Update active button styling
        UpdateActiveButton(moduleName);
    }

    private void UpdateActiveButton(string buttonName)
    {
        // Reset all buttons
        foreach (Control control in panelMenu.Controls)
        {
            if (control is Button)
            {
                control.BackColor = System.Drawing.Color.FromArgb(50, 50, 76);
                control.ForeColor = System.Drawing.Color.White;
            }
        }
        
        // Set active button
        switch (buttonName)
        {
            case "Dashboard":
                btnDashboard.BackColor = System.Drawing.Color.White;
                btnDashboard.ForeColor = System.Drawing.Color.FromArgb(50, 50, 76);
                break;
            case "Products":
                btnProducts.BackColor = System.Drawing.Color.White;
                btnProducts.ForeColor = System.Drawing.Color.FromArgb(50, 50, 76);
                break;
            case "Orders":
                btnOrders.BackColor = System.Drawing.Color.White;
                btnOrders.ForeColor = System.Drawing.Color.FromArgb(50, 50, 76);
                break;
            case "Clients":
                btnClients.BackColor = System.Drawing.Color.White;
                btnClients.ForeColor = System.Drawing.Color.FromArgb(50, 50, 76);
                break;
            case "Suppliers":
                btnSuppliers.BackColor = System.Drawing.Color.White;
                btnSuppliers.ForeColor = System.Drawing.Color.FromArgb(50, 50, 76);
                break;
            case "Deliveries":
                btnDeliveries.BackColor = System.Drawing.Color.White;
                btnDeliveries.ForeColor = System.Drawing.Color.FromArgb(50, 50, 76);
                break;
            case "Users":
                btnUsers.BackColor = System.Drawing.Color.White;
                btnUsers.ForeColor = System.Drawing.Color.FromArgb(50, 50, 76);
                break;
            case "History":
                btnHistory.BackColor = System.Drawing.Color.White;
                btnHistory.ForeColor = System.Drawing.Color.FromArgb(50, 50, 76);
                break;
        }
    }

    private void btnDashboard_Click(object sender, EventArgs e)
    {
        ShowModule("Dashboard");
    }

    private void btnProducts_Click(object sender, EventArgs e)
    {
        ShowModule("Products");
    }

    private void btnOrders_Click(object sender, EventArgs e)
    {
        ShowModule("Orders");
    }

    private void btnClients_Click(object sender, EventArgs e)
    {
        ShowModule("Clients");
    }

    private void btnSuppliers_Click(object sender, EventArgs e)
    {
        ShowModule("Suppliers");
    }

    private void btnDeliveries_Click(object sender, EventArgs e)
    {
        ShowModule("Deliveries");
    }

    private void btnUsers_Click(object sender, EventArgs e)
    {
        ShowModule("Users");
    }

    private void btnHistory_Click(object sender, EventArgs e)
    {
        ShowModule("History");
    }

    private void btnLogout_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to log out?", "Confirm Logout", 
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
        if (result == DialogResult.Yes)
        {
            // Log the logout action
            try
            {
                if (CurrentUser != null)
                {
                    using (var context = new StockContext())
                    {
                        context.Histories.Add(new StockManagementApp.Models.History
                        {
                            Action = "User logged out",
                            Date = DateTime.Now,
                            UserId = CurrentUser.UserId
                        });
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging logout: {ex.Message}");
            }
            
            // Clear current user and show login form
            CurrentUser = null;
            
            // Remove all controls
            panelContent.Controls.Clear();
            
            // Show login form
            ShowLoginForm();
        }
    }
}
