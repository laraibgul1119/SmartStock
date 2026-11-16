using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(30)]
        public string? Phone { get; set; }

        [StringLength(120), EmailAddress]
        public string? Email { get; set; }

        public List<Product> Products { get; set; } = new();
    }
}
