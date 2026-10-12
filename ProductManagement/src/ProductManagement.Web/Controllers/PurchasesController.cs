using Microsoft.AspNetCore.Mvc;
using ProductManagement.Bll.Services;
using ProductManagement.Transfer;

namespace ProductManagement.Web.Controllers;

[ApiController]
[Route("api/purchases")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchasesController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    /// <summary>Lists purchases, newest first, optionally filtered to one user.</summary>
    /// <param name="userId">Only the purchases of this user.</param>
    /// <param name="cancellationToken">Cancels the query if the client disconnects.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PurchaseListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PurchaseListItemDto>>> GetList(
        [FromQuery] int? userId,
        CancellationToken cancellationToken)
    {
        var purchases = await _purchaseService.GetListAsync(userId, cancellationToken);
        return Ok(purchases);
    }

    /// <summary>Returns one purchase with the buyer and the lines.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PurchaseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PurchaseDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var purchase = await _purchaseService.GetByIdAsync(id, cancellationToken);

        return purchase is null ? NotFound() : Ok(purchase);
    }
}
