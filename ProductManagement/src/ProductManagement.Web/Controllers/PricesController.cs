using Microsoft.AspNetCore.Mvc;
using ProductManagement.Bll.Exceptions;
using ProductManagement.Bll.Services;
using ProductManagement.Transfer;

namespace ProductManagement.Web.Controllers;

[ApiController]
[Route("api/prices")]
public class PricesController : ControllerBase
{
    private readonly IPriceService _priceService;

    public PricesController(IPriceService priceService)
    {
        _priceService = priceService;
    }

    /// <summary>The price of a product in another currency, with today's ECB rate.</summary>
    /// <param name="productId">The product.</param>
    /// <param name="currency">ISO currency code, e.g. HUF, USD.</param>
    /// <param name="cancellationToken">Cancels the call if the client disconnects.</param>
    [HttpGet("{productId:int}")]
    [ProducesResponseType(typeof(ProductPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ProductPriceDto>> GetPrice(
        int productId,
        [FromQuery] string currency = "HUF",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var price = await _priceService.GetPriceAsync(productId, currency, cancellationToken);

            return price is null ? NotFound() : Ok(price);
        }
        catch (UnknownCurrencyException ex)
        {
            return BadRequest(new { ex.Message, ex.Currency });
        }
        catch (ExchangeRateUnavailableException ex)
        {
            // Not our fault and maybe temporary: 503, so the client knows it may retry.
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { ex.Message });
        }
    }
}
