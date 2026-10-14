using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

public interface IStockService
{
    /// <summary>Active products with less than <paramref name="threshold"/> on stock, keyed by product group name.</summary>
    Task<IReadOnlyDictionary<string, IReadOnlyList<LowStockItemDto>>> GetLowStockByGroupAsync(
        int threshold,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Products from the groups the user already bought from, that the user has not bought yet.
    /// <c>null</c> if there is no such user.
    /// </summary>
    Task<IReadOnlyList<ProductListItemDto>?> GetRecommendationsAsync(int userId, CancellationToken cancellationToken = default);
}
