using Microsoft.AspNetCore.Identity;

namespace SmartStock.Models
{
    /// <summary>
    /// Extends the built-in Identity user with the shop-facing display name.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
