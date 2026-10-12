using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

public interface IPurchaseService
{
    /// <summary>Purchases, newest first, optionally filtered to one user.</summary>
    Task<IReadOnlyList<PurchaseListItemDto>> GetListAsync(
        int? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>One purchase with its lines, or <c>null</c> if there is no such purchase.</summary>
    Task<PurchaseDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
