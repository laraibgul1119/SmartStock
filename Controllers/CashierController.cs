using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Models;
using SmartStock.ViewModels;

namespace SmartStock.Controllers
{
    /// <summary>Cashier dashboard: today's counter activity and product lookup.</summary>
    [Authorize(Roles = RoleNames.Cashier)]
    public class CashierController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CashierController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var vm = new CashierDashboardViewModel
            {
                SalesToday = await _db.Sales.AsNoTracking().CountAsync(s => s.SoldAt >= today),
                RevenueToday = await _db.Sales.AsNoTracking()
                    .Where(s => s.SoldAt >= today)
                    .SumAsync(s => s.TotalAmount),

                LowStockCount = await _db.Products.AsNoTracking()
                    .CountAsync(p => p.StockQuantity > 0 && p.StockQuantity <= p.ReorderLevel),
                OutOfStockCount = await _db.Products.AsNoTracking()
                    .CountAsync(p => p.StockQuantity <= 0),

                Products = await _db.Products.AsNoTracking()
                    .OrderBy(p => p.Name)
                    .Select(p => new ProductLookupItem
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Sku = p.Sku,
                        CategoryName = p.Category.Name,
                        SellingPrice = p.SellingPrice,
                        StockQuantity = p.StockQuantity,
                        ReorderLevel = p.ReorderLevel
                    })
                    .ToListAsync(),

                RecentSales = await _db.Sales.AsNoTracking()
                    .OrderByDescending(s => s.SoldAt)
                    .Take(8)
                    .Select(s => new SaleRow
                    {
                        InvoiceNumber = s.InvoiceNumber,
                        SoldAt = s.SoldAt,
                        ItemCount = s.Items.Count,
                        TotalAmount = s.TotalAmount,
                        PaymentMethod = s.PaymentMethod,
                        CashierName = s.CashierName
                    })
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
