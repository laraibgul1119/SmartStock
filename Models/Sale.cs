using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Required, StringLength(40)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime SoldAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(20)]
        public string PaymentMethod { get; set; } = "Cash";

        [StringLength(100)]
        public string? CustomerName { get; set; }

        /// <summary>Identity user id of the cashier who recorded the sale.</summary>
        [StringLength(450)]
        public string? CashierId { get; set; }

        [StringLength(120)]
        public string? CashierName { get; set; }

        public List<SaleItem> Items { get; set; } = new();
    }
}
