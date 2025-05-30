using Microsoft.EntityFrameworkCore;
using StockManagementApp.Models;
using System;

namespace StockManagementApp
{
    public static class DbContextFactory
    {
        public static StockContext CreateContext()
        {
            try
            {
                var context = new StockContext();
                
                // Remove the problematic logging code
                // The correct way to enable logging depends on your EF Core version
                // For EF Core 3.x and above, this should be done in the OnConfiguring method
                // of your DbContext or when building the DbContextOptions
                
                return context;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating database context: {ex.Message}");
                throw new Exception($"Failed to create database context: {ex.Message}", ex);
            }
        }
    }
}
