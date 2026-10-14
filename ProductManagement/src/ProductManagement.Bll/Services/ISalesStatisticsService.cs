using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

public interface ISalesStatisticsService
{
    /// <summary>The best sellers by quantity sold.</summary>
    Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>Quantity and revenue per product group, highest revenue first.</summary>
    Task<IReadOnlyList<GroupRevenueDto>> GetRevenueByGroupAsync(CancellationToken cancellationToken = default);
}
