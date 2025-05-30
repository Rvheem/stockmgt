using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace StockManagementApp.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Phone { get; set; } = string.Empty;
        
        public string Address { get; set; } = string.Empty;
        
        public string ContactPerson { get; set; } = string.Empty;
        
        // Navigation property
        public virtual ICollection<Order>? Orders { get; set; }
    }
}
