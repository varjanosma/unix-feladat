using Microsoft.AspNetCore.Mvc;
using ProductManagement.Bll.Services;
using ProductManagement.Transfer;

namespace ProductManagement.Web.Controllers;

[ApiController]
[Route("api/product-search")]
public class ProductSearchController : ControllerBase
{
    private readonly IProductSearchService _productSearchService;

    public ProductSearchController(IProductSearchService productSearchService)
    {
        _productSearchService = productSearchService;
    }

    /// <summary>Filtered, sorted, paged product list.</summary>
    /// <remarks>Example: <c>GET /api/product-search?minPrice=50&amp;sort=PriceDescending&amp;page=1&amp;pageSize=5</c></remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ProductListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<ProductListItemDto>>> Search(
        [FromQuery] ProductSearchRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _productSearchService.SearchAsync(request, cancellationToken));
    }
}
