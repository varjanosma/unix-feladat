using Microsoft.EntityFrameworkCore;
using ProductManagement.Dal;
using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

internal sealed class StockService : IStockService
{
    private readonly ProductManagementDbContext _dbContext;

    public StockService(ProductManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyList<LowStockItemDto>>> GetLowStockByGroupAsync(
        int threshold,
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity < threshold)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new
            {
                GroupName = p.ProductGroup!.Name,
                Item = new LowStockItemDto
                {
                    ProductId = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    StockQuantity = p.StockQuantity,
                },
            })
            .ToListAsync(cancellationToken);

        // The question is "what is low in group X?", so the answer is keyed by group.
        var byGroup = new Dictionary<string, List<LowStockItemDto>>();
        foreach (var row in rows)
        {
            if (!byGroup.TryGetValue(row.GroupName, out var items))
            {
                items = new List<LowStockItemDto>();
                byGroup.Add(row.GroupName, items);
            }

            items.Add(row.Item);
        }

        return byGroup.ToDictionary(g => g.Key, g => (IReadOnlyList<LowStockItemDto>)g.Value);
    }

    public async Task<IReadOnlyList<ProductListItemDto>?> GetRecommendationsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId, cancellationToken))
        {
            return null;
        }

        var bought = await _dbContext.PurchaseItems
            .AsNoTracking()
            .Where(i => i.Purchase!.UserId == userId)
            .Select(i => new { i.ProductId, i.Product!.ProductGroupId })
            .ToListAsync(cancellationToken);

        // Two membership questions, so two HashSets: Contains is O(1), not a list scan.
        var boughtProductIds = bought.Select(b => b.ProductId).ToHashSet();
        var groupIds = bought.Select(b => b.ProductGroupId).ToHashSet();

        var candidates = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity > 0 && groupIds.Contains(p.ProductGroupId))
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                ProductGroupId = p.ProductGroupId,
                ProductGroupName = p.ProductGroup!.Name,
            })
            .ToListAsync(cancellationToken);

        return candidates
            .Where(p => !boughtProductIds.Contains(p.Id))
            .OrderBy(p => p.Name)
            .ToList();
    }
}
