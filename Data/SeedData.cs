using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartStock.Models;

namespace SmartStock.Data
{
    /// <summary>
    /// Creates the Admin/Cashier roles, the demo accounts, and a realistic
    /// catalogue with two weeks of sales history so every dashboard has data
    /// on first run. All steps are idempotent.
    /// </summary>
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await EnsureRolesAsync(roleManager);
            await EnsureUserAsync(userManager, "admin@smartstock.com", "Admin123!", "Ayesha Khan", RoleNames.Admin);
            await EnsureUserAsync(userManager, "cashier@smartstock.com", "Cashier123!", "Bilal Ahmed", RoleNames.Cashier);

            if (await db.Products.AnyAsync())
            {
                return; // catalogue already seeded
            }

            await SeedCatalogueAsync(db);
            await SeedSalesHistoryAsync(db, userManager);
        }

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in new[] { RoleNames.Admin, RoleNames.Cashier })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                return;
            }

            var user = new ApplicationUser
            {
                FullName = fullName,
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Could not seed {email}: {errors}");
            }

            await userManager.AddToRoleAsync(user, role);
        }

        // ------------------------------------------------------------ catalogue

        private static async Task SeedCatalogueAsync(ApplicationDbContext db)
        {
            var categories = new[]
            {
                new Category { Name = "Groceries", Description = "Staples, grains, oil and spices" },
                new Category { Name = "Beverages", Description = "Soft drinks, juice, tea and coffee" },
                new Category { Name = "Snacks", Description = "Chips, biscuits, chocolate and namkeen" },
                new Category { Name = "Stationery", Description = "Pens, notebooks and school supplies" },
                new Category { Name = "Household", Description = "Cleaning and personal care essentials" },
                new Category { Name = "Dairy", Description = "Milk, yogurt, butter and eggs" }
            };
            db.Categories.AddRange(categories);

            var suppliers = new[]
            {
                new Supplier { Name = "Metro Wholesale", ContactPerson = "Imran Sheikh", Phone = "0300-1234567", Email = "sales@metrowholesale.pk" },
                new Supplier { Name = "Sunrise Distributors", ContactPerson = "Sana Iqbal", Phone = "0321-9876543", Email = "orders@sunrisedist.pk" },
                new Supplier { Name = "Green Valley Traders", ContactPerson = "Kamran Yousuf", Phone = "0333-5556677", Email = "contact@greenvalley.pk" },
                new Supplier { Name = "Office Base Co.", ContactPerson = "Hina Farooq", Phone = "0311-2223344", Email = "support@officebase.pk" }
            };
            db.Suppliers.AddRange(suppliers);

            await db.SaveChangesAsync();

            var cat = categories.ToDictionary(c => c.Name, c => c.Id);
            var sup = suppliers.ToDictionary(s => s.Name, s => s.Id);

            Product P(string name, string sku, string category, string? supplier,
                      decimal cost, decimal price, int stock, int reorder, string unit = "pcs") => new()
            {
                Name = name,
                Sku = sku,
                CategoryId = cat[category],
                SupplierId = supplier is null ? null : sup[supplier],
                CostPrice = cost,
                SellingPrice = price,
                StockQuantity = stock,
                ReorderLevel = reorder,
                Unit = unit,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var products = new[]
            {
                // Groceries
                P("Basmati Rice 5kg", "GRC-001", "Groceries", "Metro Wholesale", 780, 950, 42, 10, "bag"),
                P("Sunflower Oil 1L", "GRC-002", "Groceries", "Metro Wholesale", 410, 499, 8, 12),
                P("Wheat Flour 10kg", "GRC-003", "Groceries", "Green Valley Traders", 1150, 1399, 25, 8, "bag"),
                P("Sugar 1kg", "GRC-004", "Groceries", "Green Valley Traders", 155, 189, 60, 15),
                P("Tea Leaves 250g", "GRC-005", "Groceries", "Sunrise Distributors", 420, 545, 4, 10),
                P("Red Chilli Powder 200g", "GRC-006", "Groceries", "Green Valley Traders", 180, 240, 33, 10),

                // Beverages
                P("Cola 500ml", "BEV-001", "Beverages", "Sunrise Distributors", 55, 80, 144, 36, "bottle"),
                P("Mango Juice 1L", "BEV-002", "Beverages", "Sunrise Distributors", 240, 320, 18, 12),
                P("Mineral Water 1.5L", "BEV-003", "Beverages", "Metro Wholesale", 60, 90, 72, 24, "bottle"),
                P("Instant Coffee 100g", "BEV-004", "Beverages", "Metro Wholesale", 520, 675, 6, 8),

                // Snacks
                P("Potato Chips 50g", "SNK-001", "Snacks", "Sunrise Distributors", 40, 60, 96, 24),
                P("Chocolate Bar 45g", "SNK-002", "Snacks", "Sunrise Distributors", 95, 140, 0, 12),
                P("Salted Biscuits 200g", "SNK-003", "Snacks", "Metro Wholesale", 110, 155, 40, 10),
                P("Namkeen Mix 400g", "SNK-004", "Snacks", "Green Valley Traders", 260, 340, 7, 10),

                // Stationery
                P("Ball Pen (Blue)", "STN-001", "Stationery", "Office Base Co.", 12, 20, 250, 50),
                P("A4 Notebook 100pg", "STN-002", "Stationery", "Office Base Co.", 95, 150, 5, 15),
                P("HB Pencils Pack of 10", "STN-003", "Stationery", "Office Base Co.", 80, 120, 65, 20),
                P("Glue Stick 20g", "STN-004", "Stationery", "Office Base Co.", 55, 90, 30, 12),
                P("Highlighter Set (4pc)", "STN-005", "Stationery", "Office Base Co.", 180, 260, 0, 8),

                // Household
                P("Dishwashing Liquid 500ml", "HHD-001", "Household", "Green Valley Traders", 175, 240, 28, 10),
                P("Laundry Powder 1kg", "HHD-002", "Household", "Metro Wholesale", 320, 425, 16, 8),
                P("Shampoo 200ml", "HHD-003", "Household", "Sunrise Distributors", 285, 380, 9, 10),
                P("Bar Soap (Triple Pack)", "HHD-004", "Household", "Sunrise Distributors", 150, 210, 45, 15),

                // Dairy
                P("Fresh Milk 1L", "DRY-001", "Dairy", "Green Valley Traders", 210, 250, 34, 20, "carton"),
                P("Yogurt 500g", "DRY-002", "Dairy", "Green Valley Traders", 120, 160, 6, 12),
                P("Butter 250g", "DRY-003", "Dairy", "Metro Wholesale", 480, 620, 11, 8),
                P("Eggs (Dozen)", "DRY-004", "Dairy", "Green Valley Traders", 260, 330, 22, 12, "box")
            };

            db.Products.AddRange(products);
            await db.SaveChangesAsync();
        }

        // -------------------------------------------------------- sales history

        private static async Task SeedSalesHistoryAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            var cashier = await userManager.FindByEmailAsync("cashier@smartstock.com");
            var products = await db.Products.ToListAsync();

            // Deterministic history so the 14-day chart always looks the same on a fresh DB.
            var rng = new Random(20260101);
            var today = DateTime.Today;
            var invoiceSeq = 0;

            for (var daysBack = 13; daysBack >= 0; daysBack--)
            {
                var day = today.AddDays(-daysBack);
                var salesThatDay = rng.Next(3, 9); // 3–8 sales per day

                for (var i = 0; i < salesThatDay; i++)
                {
                    var lineCount = rng.Next(1, 4); // 1–3 lines per sale
                    var sale = new Sale
                    {
                        InvoiceNumber = $"INV-{day:yyyyMMdd}-{++invoiceSeq:D4}",
                        SoldAt = day.AddHours(rng.Next(9, 21)).AddMinutes(rng.Next(0, 60)),
                        PaymentMethod = rng.Next(4) == 0 ? "Card" : "Cash",
                        CustomerName = rng.Next(3) == 0 ? "Walk-in" : null,
                        CashierId = cashier?.Id,
                        CashierName = cashier?.FullName
                    };

                    for (var line = 0; line < lineCount; line++)
                    {
                        var product = products[rng.Next(products.Count)];
                        var quantity = rng.Next(1, 4);

                        sale.Items.Add(new SaleItem
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Quantity = quantity,
                            UnitPrice = product.SellingPrice
                        });
                    }

                    sale.TotalAmount = sale.Items.Sum(i => i.LineTotal);
                    db.Sales.Add(sale);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
