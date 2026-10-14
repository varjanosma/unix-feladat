using Microsoft.AspNetCore.Mvc;
using ProductManagement.Bll.Exceptions;
using ProductManagement.Bll.Services;
using ProductManagement.Transfer;

namespace ProductManagement.Web.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPurchaseService _purchaseService;
    private readonly ICheckoutService _checkoutService;

    public OrdersController(IOrderService orderService, IPurchaseService purchaseService, ICheckoutService checkoutService)
    {
        _orderService = orderService;
        _purchaseService = purchaseService;
        _checkoutService = checkoutService;
    }

    /// <summary>Places an order. Returns the new purchase.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PurchaseDetailDto>> PlaceOrder(
        PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        // Only the exceptions we expect and can explain are caught, each with its own status code.
        // Anything else (a bug, the database is down) is not caught here and becomes a 500.
        try
        {
            var purchaseId = await _orderService.PlaceOrderAsync(request, cancellationToken);
            var purchase = await _purchaseService.GetByIdAsync(purchaseId, cancellationToken);

            return CreatedAtAction(nameof(PurchasesController.GetById), "Purchases", new { id = purchaseId }, purchase);
        }
        catch (OrderValidationException ex)
        {
            return BadRequest(new { ex.Message, ex.Errors });
        }
        catch (InsufficientStockException ex)
        {
            return Conflict(new { ex.Message, ex.Sku, ex.Requested, ex.Available });
        }
    }

    /// <summary>Places an order and returns its total in the requested currency.</summary>
    /// <param name="request">The order.</param>
    /// <param name="currency">ISO currency code, e.g. HUF, USD. Prices are stored in EUR.</param>
    /// <param name="cancellationToken">Cancels the call if the client disconnects.</param>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(CheckoutResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CheckoutResultDto>> Checkout(
        PlaceOrderRequest request,
        [FromQuery] string currency = "EUR",
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await _checkoutService.CheckoutAsync(request, currency, cancellationToken));
        }
        catch (OrderValidationException ex)
        {
            return BadRequest(new { ex.Message, ex.Errors });
        }
        catch (UnknownCurrencyException ex)
        {
            return BadRequest(new { ex.Message, ex.Currency });
        }
        catch (InsufficientStockException ex)
        {
            return Conflict(new { ex.Message, ex.Sku, ex.Requested, ex.Available });
        }
        catch (ExchangeRateUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { ex.Message });
        }
    }
}
