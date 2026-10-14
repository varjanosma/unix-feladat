using Microsoft.AspNetCore.Mvc;
using ProductManagement.Bll.Services;
using ProductManagement.Transfer;

namespace ProductManagement.Web.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    /// <summary>Active products running out, grouped by product group name.</summary>
    /// <param name="threshold">Products with less stock than this are listed.</param>
    /// <param name="cancellationToken">Cancels the query if the client disconnects.</param>
    [HttpGet("low")]
    [ProducesResponseType(typeof(IReadOnlyDictionary<string, IReadOnlyList<LowStockItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyDictionary<string, IReadOnlyList<LowStockItemDto>>>> GetLowStock(
        [FromQuery] int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _stockService.GetLowStockByGroupAsync(threshold, cancellationToken));
    }

    /// <summary>Products the user may be interested in.</summary>
    [HttpGet("recommendations/{userId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProductListItemDto>>> GetRecommendations(
        int userId,
        CancellationToken cancellationToken)
    {
        var products = await _stockService.GetRecommendationsAsync(userId, cancellationToken);

        return products is null ? NotFound() : Ok(products);
    }
}
