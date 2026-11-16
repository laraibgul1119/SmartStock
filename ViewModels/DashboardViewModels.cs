using SmartStock.Models;

namespace SmartStock.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Headline numbers
        public int TotalProducts { get; set; }
        public int TotalUnitsInStock { get; set; }
        public decimal StockValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }

        // Sales
        public int SalesToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int SalesThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }

        // Charts
        public List<DailyRevenuePoint> RevenueLast14Days { get; set; } = new();
        public List<CategoryStockPoint> StockByCategory { get; set; } = new();

        // Tables
        public List<StockAlertItem> StockAlerts { get; set; } = new();
        public List<SaleRow> RecentSales { get; set; } = new();
        public List<TopProductRow> TopProducts { get; set; } = new();
    }

    public class CashierDashboardViewModel
    {
        public int SalesToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }

        public List<ProductLookupItem> Products { get; set; } = new();
        public List<SaleRow> RecentSales { get; set; } = new();
    }

    public class DailyRevenuePoint
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Count { get; set; }
    }

    public class CategoryStockPoint
    {
        public string Name { get; set; } = string.Empty;
        public int Units { get; set; }
    }

    public class StockAlertItem
    {
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public bool IsOutOfStock { get; set; }
    }

    public class SaleRow
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime SoldAt { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? CashierName { get; set; }
    }

    public class TopProductRow
    {
        public string Name { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ProductLookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public bool IsOutOfStock => StockQuantity <= 0;
        public bool IsLowStock => StockQuantity > 0 && StockQuantity <= ReorderLevel;
    }
}
