using Microsoft.EntityFrameworkCore;
using ProductManagement.Transfer;
using ProductManagement.Dal;

namespace ProductManagement.Bll.Services;

internal sealed class UserService : IUserService
{
    private readonly ProductManagementDbContext _dbContext;

    public UserService(ProductManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserListItemDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email,
                RegisteredAtUtc = u.RegisteredAtUtc,
                PurchaseCount = u.Purchases.Count,
                TotalSpent = u.Purchases
                    .SelectMany(p => p.Items)
                    .Sum(i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserDetailDto
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email,
                RegisteredAtUtc = u.RegisteredAtUtc,
                TotalSpent = u.Purchases
                    .SelectMany(p => p.Items)
                    .Sum(i => (decimal?)(i.Quantity * i.UnitPrice)) ?? 0m,
                Purchases = u.Purchases
                    .OrderByDescending(p => p.PurchasedAtUtc)
                    .Select(p => new PurchaseDto
                    {
                        Id = p.Id,
                        PurchasedAtUtc = p.PurchasedAtUtc,
                        Note = p.Note,
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
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
