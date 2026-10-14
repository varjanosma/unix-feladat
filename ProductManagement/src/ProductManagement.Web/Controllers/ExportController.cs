using Microsoft.AspNetCore.Mvc;
using ProductManagement.Bll.Services;

namespace ProductManagement.Web.Controllers;

[ApiController]
[Route("api/export")]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>Downloads the active product catalogue as a JSON file.</summary>
    [HttpGet("catalog")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCatalog(CancellationToken cancellationToken)
    {
        var json = await _exportService.ExportCatalogJsonAsync(cancellationToken);

        return File(json, "application/json", "catalog.json");
    }

    /// <summary>The invoice of a purchase as XML, in the format of the invoicing system.</summary>
    [HttpGet("invoices/{purchaseId:int}")]
    [Produces("application/xml")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoice(int purchaseId, CancellationToken cancellationToken)
    {
        var xml = await _exportService.GetInvoiceXmlAsync(purchaseId, cancellationToken);

        return xml is null ? NotFound() : File(xml, "application/xml");
    }
}
