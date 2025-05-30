using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagementApp.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        
        public string? Status { get; set; } = "Pending";
        
        public int ClientId { get; set; }
        
        // Navigation properties
        public virtual Client? Client { get; set; }
        
        public virtual ICollection<OrderItem>? OrderItems { get; set; }
        // Add Items as an alias for OrderItems
        [NotMapped]
        public virtual ICollection<OrderItem>? Items 
        { 
            get => OrderItems; 
            set => OrderItems = value; 
        }
        
        public virtual Delivery? Delivery { get; set; }
    }
    
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        
        public int OrderId { get; set; }
        
        public int ProductId { get; set; }
        
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        // Add UnitPrice as an alias for Price
        [NotMapped]
        public decimal UnitPrice 
        { 
            get => Price; 
            set => Price = value; 
        }
        
        // Add Subtotal property
        [NotMapped]
        public decimal Subtotal => Quantity * Price;
        
        // Navigation properties
        public virtual Order? Order { get; set; }
        
        public virtual Product? Product { get; set; }
    }
}
