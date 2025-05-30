using System;
using System.ComponentModel.DataAnnotations;

namespace StockManagementApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
        public DateTime? LastActivity { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
