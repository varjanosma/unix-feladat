using Microsoft.EntityFrameworkCore;
using ProductManagement.Transfer;
using ProductManagement.Dal;

namespace ProductManagement.Bll.Services;

internal sealed class ProductService : IProductService
{
    private readonly ProductManagementDbContext _dbContext;

    public ProductService(ProductManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductListItemDto>> GetListAsync(
        int? productGroupId = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products.AsNoTracking();

        // Conditional filtering: the query is only built up here, nothing runs yet.
        // That is the point of IQueryable - execution happens at ToListAsync.
        if (productGroupId.HasValue)
        {
            query = query.Where(p => p.ProductGroupId == productGroupId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Sku.Contains(term));
        }

        return await query
            .OrderBy(p => p.Name)
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
    }

    public async Task<ProductDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Returning null is not an error here: "no such product" is a normal outcome.
        // Turning it into a 404 is the controller's job.
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDetailDto
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                CreatedAtUtc = p.CreatedAtUtc,
                ProductGroupId = p.ProductGroupId,
                ProductGroupName = p.ProductGroup!.Name,
                Manufacturer = p.Info!.Manufacturer,
                Description = p.Info.Description,
                WarrantyMonths = p.Info.WarrantyMonths,
                WeightKg = p.Info.WeightKg,
                CountryOfOrigin = p.Info.CountryOfOrigin,
                TotalSoldQuantity = p.PurchaseItems.Sum(i => (int?)i.Quantity) ?? 0,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
