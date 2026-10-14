using Microsoft.AspNetCore.Mvc;
using ProductManagement.Bll.Services;
using ProductManagement.Transfer;

namespace ProductManagement.Web.Controllers;

[ApiController]
[Route("api/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly ISalesStatisticsService _statisticsService;

    public StatisticsController(ISalesStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>Best sellers by quantity sold.</summary>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(IReadOnlyList<TopProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TopProductDto>>> GetTopProducts(
        [FromQuery] int count = 5,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _statisticsService.GetTopProductsAsync(count, cancellationToken));
    }

    /// <summary>Quantity and revenue per product group.</summary>
    [HttpGet("revenue-by-group")]
    [ProducesResponseType(typeof(IReadOnlyList<GroupRevenueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GroupRevenueDto>>> GetRevenueByGroup(CancellationToken cancellationToken)
    {
        return Ok(await _statisticsService.GetRevenueByGroupAsync(cancellationToken));
    }
}
