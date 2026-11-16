using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(40)]
        public string Sku { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        public int StockQuantity { get; set; }

        /// <summary>Stock level at which the product is flagged for reordering.</summary>
        public int ReorderLevel { get; set; }

        [StringLength(20)]
        public string Unit { get; set; } = "pcs";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsOutOfStock => StockQuantity <= 0;

        public bool IsLowStock => StockQuantity > 0 && StockQuantity <= ReorderLevel;
    }
}
