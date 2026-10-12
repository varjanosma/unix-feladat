using Microsoft.AspNetCore.Mvc;
using ProductManagement.Transfer;
using ProductManagement.Bll.Services;

namespace ProductManagement.Web.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>Lists products, optionally filtered by group and by a search term.</summary>
    /// <param name="productGroupId">Only products of this group.</param>
    /// <param name="search">Matched against the product name and SKU.</param>
    /// <param name="cancellationToken">Cancels the query if the client disconnects.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductListItemDto>>> GetList(
        [FromQuery] int? productGroupId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetListAsync(productGroupId, search, cancellationToken);
        return Ok(products);
    }

    /// <summary>Returns one product with its details.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            // "Not found" is a 404, not a 200 with an empty body. The client can only
            // tell the two cases apart from the status code.
            _logger.LogInformation("Product {ProductId} was not found", id);
            return NotFound();
        }

        return Ok(product);
    }
}
