using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

public interface IProductSearchService
{
    /// <summary>Filtered, sorted, paged product list.</summary>
    Task<PagedResultDto<ProductListItemDto>> SearchAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken = default);
}
