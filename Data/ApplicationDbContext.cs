using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartStock.Models;

namespace SmartStock.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>(e =>
            {
                e.HasIndex(c => c.Name).IsUnique();
            });

            builder.Entity<Supplier>(e =>
            {
                e.HasIndex(s => s.Name).IsUnique();
            });

            builder.Entity<Product>(e =>
            {
                e.HasIndex(p => p.Sku).IsUnique();
                e.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(p => p.Supplier)
                    .WithMany(s => s.Products)
                    .HasForeignKey(p => p.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Sale>(e =>
            {
                e.HasIndex(s => s.InvoiceNumber).IsUnique();
            });

            builder.Entity<SaleItem>(e =>
            {
                e.HasOne(i => i.Sale)
                    .WithMany(s => s.Items)
                    .HasForeignKey(i => i.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(i => i.Product)
                    .WithMany()
                    .HasForeignKey(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // LineTotal is computed from quantity * unit price.
                e.Ignore(i => i.LineTotal);
            });
        }
    }
}
