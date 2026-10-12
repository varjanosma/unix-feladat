using Microsoft.EntityFrameworkCore;
using ProductManagement.Dal;
using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

internal sealed class PurchaseService : IPurchaseService
{
    private readonly ProductManagementDbContext _dbContext;

    public PurchaseService(ProductManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PurchaseListItemDto>> GetListAsync(
        int? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Purchases.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        return await query
            .OrderByDescending(p => p.PurchasedAtUtc)
            .Select(p => new PurchaseListItemDto
            {
                Id = p.Id,
                PurchasedAtUtc = p.PurchasedAtUtc,
                Note = p.Note,
                UserId = p.UserId,
                UserFullName = p.User!.FullName,
                ItemCount = p.Items.Count,
                TotalQuantity = p.Items.Sum(i => (int?)i.Quantity) ?? 0,
                Total = p.Items.Sum(i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PurchaseDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Purchases
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PurchaseDetailDto
            {
                Id = p.Id,
                PurchasedAtUtc = p.PurchasedAtUtc,
                Note = p.Note,
                UserId = p.UserId,
                UserFullName = p.User!.FullName,
                UserEmail = p.User.Email,
                Total = p.Items.Sum(i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m,
                Items = p.Items
                    .OrderBy(i => i.Id)
                    .Select(i => new PurchaseItemDto
                    {
                        ProductId = i.ProductId,
                        Sku = i.Product!.Sku,
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        LineTotal = i.Quantity * i.UnitPrice,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
