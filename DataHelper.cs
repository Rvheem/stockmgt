using Microsoft.EntityFrameworkCore;
using StockManagementApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagementApp
{
    public static class DataHelper
    {
        // User-related methods
        public static User AuthenticateUser(string username, string password)
        {
            using (var context = new StockContext())
            {
                // This also uses Username, not Email
                return context.Users
                    .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);
            }
        }
        
        public static void LogUserAction(int userId, string action)
        {
            using (var context = new StockContext())
            {
                var history = new History
                {
                    UserId = userId,
                    Action = action,
                    Date = DateTime.Now
                };
                
                context.Histories.Add(history);
                context.SaveChanges();
            }
        }
        
        // Product-related methods
        public static List<Product> GetLowStockProducts()
        {
            using (var context = new StockContext())
            {
                return context.Products
                    .Where(p => p.Quantity <= p.Threshold)
                    .Include(p => p.Supplier)
                    .ToList();
            }
        }
        
        public static List<Product> GetExpiringProducts(int daysThreshold = 30)
        {
            var thresholdDate = DateTime.Now.AddDays(daysThreshold);
            
            using (var context = new StockContext())
            {
                return context.Products
                    .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate <= thresholdDate)
                    .Include(p => p.Supplier)
                    .ToList();
            }
        }
        
        // Order processing methods
        public static void ProcessOrder(Order order)
        {
            using (var context = new StockContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // Add the order
                        context.Orders.Add(order);
                        context.SaveChanges();
                        
                        // Update product quantities
                        foreach (var item in order.Items)
                        {
                            var product = context.Products.Find(item.ProductId);
                            if (product != null)
                            {
                                product.Quantity -= item.Quantity;
                                if (product.Quantity < 0)
                                {
                                    throw new InvalidOperationException($"Insufficient stock for product: {product.Name}");
                                }
                            }
                        }
                        
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
