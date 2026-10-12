using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserListItemDto>> GetListAsync(CancellationToken cancellationToken = default);

    /// <summary>One user with the purchase history, or <c>null</c> if there is no such user.</summary>
    Task<UserDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
