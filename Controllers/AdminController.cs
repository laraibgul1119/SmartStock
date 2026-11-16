using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Models;
using SmartStock.ViewModels;

namespace SmartStock.Controllers
{
    /// <summary>Shop-owner dashboard: stock, sales and business performance.</summary>
    [Authorize(Roles = RoleNames.Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var fortnightAgo = today.AddDays(-13);

            var salesQuery = _db.Sales.AsNoTracking();
            var productsQuery = _db.Products.AsNoTracking();

            var vm = new AdminDashboardViewModel
            {
                TotalProducts = await productsQuery.CountAsync(),
                TotalUnitsInStock = await productsQuery.SumAsync(p => p.StockQuantity),
                StockValue = await productsQuery.SumAsync(p => p.CostPrice * p.StockQuantity),
                LowStockCount = await productsQuery.CountAsync(p => p.StockQuantity > 0 && p.StockQuantity <= p.ReorderLevel),
                OutOfStockCount = await productsQuery.CountAsync(p => p.StockQuantity <= 0),

                SalesToday = await salesQuery.CountAsync(s => s.SoldAt >= today),
                RevenueToday = await salesQuery.Where(s => s.SoldAt >= today).SumAsync(s => s.TotalAmount),
                SalesThisMonth = await salesQuery.CountAsync(s => s.SoldAt >= monthStart),
                RevenueThisMonth = await salesQuery.Where(s => s.SoldAt >= monthStart).SumAsync(s => s.TotalAmount),

                // Chart: one bucket per calendar day for the last 14 days (gaps become zero).
                RevenueLast14Days = await salesQuery
                    .Where(s => s.SoldAt >= fortnightAgo)
                    .GroupBy(s => s.SoldAt.Date)
                    .Select(g => new DailyRevenuePoint { Date = g.Key, Revenue = g.Sum(s => s.TotalAmount), Count = g.Count() })
                    .OrderBy(p => p.Date)
                    .ToListAsync(),

                StockByCategory = await productsQuery
                    .GroupBy(p => p.Category.Name)
                    .Select(g => new CategoryStockPoint { Name = g.Key, Units = g.Sum(p => p.StockQuantity) })
                    .OrderByDescending(c => c.Units)
                    .ToListAsync(),

                StockAlerts = await productsQuery
                    .Where(p => p.StockQuantity <= p.ReorderLevel)
                    .OrderBy(p => p.StockQuantity)
                    .ThenBy(p => p.Name)
                    .Select(p => new StockAlertItem
                    {
                        Name = p.Name,
                        Sku = p.Sku,
                        CategoryName = p.Category.Name,
                        StockQuantity = p.StockQuantity,
                        ReorderLevel = p.ReorderLevel,
                        IsOutOfStock = p.StockQuantity <= 0
                    })
                    .Take(8)
                    .ToListAsync(),

                RecentSales = await salesQuery
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
                    .ToListAsync(),

                TopProducts = await _db.SaleItems.AsNoTracking()
                    .GroupBy(i => i.ProductName)
                    .Select(g => new TopProductRow
                    {
                        Name = g.Key,
                        UnitsSold = g.Sum(i => i.Quantity),
                        Revenue = g.Sum(i => i.UnitPrice * i.Quantity)
                    })
                    .OrderByDescending(p => p.Revenue)
                    .Take(5)
                    .ToListAsync()
            };

            // Fill missing days so the chart x-axis is continuous.
            var byDate = vm.RevenueLast14Days.ToDictionary(p => p.Date);
            vm.RevenueLast14Days = Enumerable.Range(0, 14)
                .Select(i => fortnightAgo.AddDays(i))
                .Select(d => byDate.TryGetValue(d, out var point)
                    ? point
                    : new DailyRevenuePoint { Date = d, Revenue = 0, Count = 0 })
                .ToList();

            return View(vm);
        }
    }
}
