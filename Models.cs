using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagementApp.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int Threshold { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Added missing Price property
        public DateTime? ExpiryDate { get; set; }
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
    }

    public class Order
    {
        public Order()
        {
            Items = new List<OrderItem>();
        }
        
        [Key]
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        public ICollection<OrderItem> Items { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
    }
    
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
    
    public class Client
    {
        [Key]
        public int ClientId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
    }
    
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? ContactPerson { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
    
    public class Delivery
    {
        [Key]
        public int DeliveryId { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, In Transit, Delivered, Cancelled
        public string? Notes { get; set; }
    }
    
    // User model
public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
    
    public class History
    {
        [Key]
        public int HistoryId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public User? User { get; set; }
    }

    public class UserAction
    {
        [Key]
        public int ActionId { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        [Required]
        public string ActionDescription { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
