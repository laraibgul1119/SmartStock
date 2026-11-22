using Dapper;
using Microsoft.Data.SqlClient;
using SmartStock.Models;

namespace SmartStock.Models.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly IConfiguration _configuration;

    public ProductRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<LowStockProductDto>> GetLowStockAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found.");

        const string sql = """
            SELECT
                Id,
                Name,
                Sku,
                StockQuantity,
                ReorderLevel
            FROM Products
            WHERE StockQuantity <= ReorderLevel
            ORDER BY StockQuantity ASC, Name ASC;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            sql,
            cancellationToken: cancellationToken);

        var products = await connection.QueryAsync<LowStockProductDto>(command);

        return products.AsList();
    }
}