using Microsoft.AspNetCore.Mvc;
using ProductManagement.Transfer;
using ProductManagement.Bll.Services;

namespace ProductManagement.Web.Controllers;

[ApiController]
[Route("api/product-groups")]
public class ProductGroupsController : ControllerBase
{
    private readonly IProductGroupService _productGroupService;

    public ProductGroupsController(IProductGroupService productGroupService)
    {
        _productGroupService = productGroupService;
    }

    /// <summary>Lists every product group with the number of products in it.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductGroupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductGroupDto>>> GetAll(CancellationToken cancellationToken)
    {
        var groups = await _productGroupService.GetAllAsync(cancellationToken);
        return Ok(groups);
    }
}
