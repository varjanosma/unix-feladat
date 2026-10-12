using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

public interface IProductService
{
    /// <summary>Products, optionally filtered by group and by a free-text search term.</summary>
    Task<IReadOnlyList<ProductListItemDto>> GetListAsync(
        int? productGroupId = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>One product with its details, or <c>null</c> if there is no such product.</summary>
    Task<ProductDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
