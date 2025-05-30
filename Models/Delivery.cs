using System;
using System.ComponentModel.DataAnnotations;

namespace StockManagementApp.Models
{
    public class Delivery
    {
        public int DeliveryId { get; set; }
        
        public int OrderId { get; set; }
        
        public DateTime DeliveryDate { get; set; } = DateTime.Now;
        
        public string? Status { get; set; } = "Pending";
        
        // Navigation property
        public virtual Order? Order { get; set; }
    }
}
