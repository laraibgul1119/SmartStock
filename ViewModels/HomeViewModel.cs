namespace SmartStock.ViewModels
{
    /// <summary>Data shown on the public home page — all read live from the database.</summary>
    public class HomeViewModel
    {
        public int TotalProducts { get; set; }
        public int CategoryCount { get; set; }
        public int SupplierCount { get; set; }
        public int TotalUnitsInStock { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int SalesToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int SalesWeek { get; set; }
        public decimal RevenueWeek { get; set; }
    }
}
