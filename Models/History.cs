using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagementApp.Models
{
    public class History
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
