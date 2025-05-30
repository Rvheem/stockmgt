using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagementApp.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public string Reference { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        public int Quantity { get; set; }
        
        public int Threshold { get; set; } = 10;
        
        public DateTime? ExpiryDate { get; set; }
        
        public int? SupplierId { get; set; }
        
        public string? Status { get; set; } = "In Stock";
        
        // Navigation property
        public virtual Supplier? Supplier { get; set; }
    }
}
