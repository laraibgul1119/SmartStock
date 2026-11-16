using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        public List<Product> Products { get; set; } = new();
    }
}
