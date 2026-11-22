namespace SmartStock.Models;

public sealed class LowStockProductDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Sku { get; init; } = string.Empty;

    public int StockQuantity { get; init; }

    public int ReorderLevel { get; init; }

    public bool IsOutOfStock => StockQuantity <= 0;
}