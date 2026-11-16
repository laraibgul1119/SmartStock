using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStock.Data;
using SmartStock.Models;
using SmartStock.ViewModels;
using System.Diagnostics;

namespace SmartStock.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var weekAgo = today.AddDays(-6);

            var products = _db.Products.AsNoTracking();
            var sales = _db.Sales.AsNoTracking();

            var vm = new HomeViewModel
            {
                TotalProducts = await products.CountAsync(),
                CategoryCount = await _db.Categories.AsNoTracking().CountAsync(),
                SupplierCount = await _db.Suppliers.AsNoTracking().CountAsync(),
                TotalUnitsInStock = await products.SumAsync(p => p.StockQuantity),
                LowStockCount = await products.CountAsync(p => p.StockQuantity > 0 && p.StockQuantity <= p.ReorderLevel),
                OutOfStockCount = await products.CountAsync(p => p.StockQuantity <= 0),
                SalesToday = await sales.CountAsync(s => s.SoldAt >= today),
                RevenueToday = await sales.Where(s => s.SoldAt >= today).SumAsync(s => s.TotalAmount),
                SalesWeek = await sales.CountAsync(s => s.SoldAt >= weekAgo),
                RevenueWeek = await sales.Where(s => s.SoldAt >= weekAgo).SumAsync(s => s.TotalAmount)
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
