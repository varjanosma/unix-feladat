using Microsoft.AspNetCore.Mvc;
using ProductManagement.Transfer;
using ProductManagement.Bll.Services;

namespace ProductManagement.Web.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Lists users with the number of purchases and the total amount spent.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> GetList(CancellationToken cancellationToken)
    {
        var users = await _userService.GetListAsync(cancellationToken);
        return Ok(users);
    }

    /// <summary>Returns one user together with the purchase history.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }
}
