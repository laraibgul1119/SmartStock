using SmartStock.Models;

namespace SmartStock.Models.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<LowStockProductDto>> GetLowStockAsync(
        CancellationToken cancellationToken = default);
}