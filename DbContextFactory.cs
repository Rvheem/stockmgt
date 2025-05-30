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
                return new StockContext();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating database context: {ex.Message}");
                throw;
            }
        }
    }
}
