using Microsoft.EntityFrameworkCore;
using ProductManagement.Dal;
using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

internal sealed class SalesStatisticsService : ISalesStatisticsService
{
    private readonly ProductManagementDbContext _dbContext;

    public SalesStatisticsService(ProductManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PurchaseItems
            .AsNoTracking()
            .GroupBy(i => new { i.ProductId, i.Product!.Sku, i.Product.Name })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId,
                Sku = g.Key.Sku,
                Name = g.Key.Name,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice),
            })
            .OrderByDescending(t => t.QuantitySold)
            .ThenByDescending(t => t.Revenue)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GroupRevenueDto>> GetRevenueByGroupAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PurchaseItems
            .AsNoTracking()
            .GroupBy(i => new { i.Product!.ProductGroupId, i.Product.ProductGroup!.Name })
            .Select(g => new GroupRevenueDto
            {
                ProductGroupId = g.Key.ProductGroupId,
                ProductGroupName = g.Key.Name,
                QuantitySold = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.Quantity * i.UnitPrice),
            })
            .OrderByDescending(r => r.Revenue)
            .ToListAsync(cancellationToken);
    }
}
