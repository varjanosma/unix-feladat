using Microsoft.AspNetCore.Mvc;
using ProductManagement.Bll.Services;

namespace ProductManagement.Web.Controllers;

/// <summary>
/// Health endpoint. It only proves that the layers compose and that DI works.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IInfoService _infoService;

    public HealthController(IInfoService infoService)
    {
        _infoService = infoService;
    }

    /// <summary>Returns the current status of the API.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get()
    {
        return Ok(new HealthResponse
        {
            Status = "Healthy",
            UtcNow = DateTimeOffset.UtcNow,
            Runtime = Environment.Version.ToString(),
            BllInfo = _infoService.GetBllInfo()
        });
    }
}

/// <summary>Response of the health endpoint.</summary>
public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset UtcNow { get; set; }
    public string Runtime { get; set; } = string.Empty;
    public string BllInfo { get; set; } = string.Empty;
}
