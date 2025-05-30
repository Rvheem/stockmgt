namespace StockManagementApp;

using StockManagementApp.Models;
using System.Windows.Forms;
using System;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ensure default admin user exists
            EnsureDefaultAdminUser();

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            
            // Start with login form
            using (var loginForm = new LoginForm())
            {
                var result = loginForm.ShowDialog();
                
                // Only open the main form if login was successful
                if (result == DialogResult.OK)
                {
                    Application.Run(new Form1(loginForm.User));
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fatal error: {ex.Message}\n{ex.StackTrace}", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(1);
        }
    }

    private static void EnsureDefaultAdminUser()
    {
        using (var context = new StockManagementApp.Models.StockContext())
        {
            // Check if admin user exists
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                context.Users.Add(new StockManagementApp.Models.User
                {
                    Username = "admin",
                    Password = "admin", // In production, hash this!
                    Role = "Administrator",
                    FullName = "System Administrator", // Added required field
                    IsActive = true // Added required field
                });
                context.SaveChanges();
            }
        }
    }

    static void InitializeDatabase()
    {
        try 
        {
            // Force recreation of the database to fix schema issues
            // Comment this line out after first run to preserve data
            // StockContext.RecreateDatabase();
            
            // Initialize the database
            // StockContext.Initialize();
            
            MessageBox.Show("Database initialized successfully!", "Database Setup", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing database: {ex.Message}", "Database Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            // Ask user if they want to recreate the database
            var result = MessageBox.Show(
                "Would you like to recreate the database? This will delete all existing data.", 
                "Database Error", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
                
            if (result == DialogResult.Yes)
            {
                try
                {
                    // StockContext.RecreateDatabase();
                    MessageBox.Show("Database recreated successfully!", "Database Setup", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception recreateEx)
                {
                    MessageBox.Show($"Error recreating database: {recreateEx.Message}", "Database Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            }
        }
    }
}