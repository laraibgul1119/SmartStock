using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models.Repositories;

namespace SmartStock.Controllers;

[Authorize]
public sealed class InventoryController : Controller
{
    private readonly IProductRepository _productRepository;

    public InventoryController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<IActionResult> LowStock(
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetLowStockAsync(cancellationToken);

        return Json(products);
    }
}