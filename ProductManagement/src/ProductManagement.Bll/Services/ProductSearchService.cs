using Microsoft.EntityFrameworkCore;
using ProductManagement.Dal;
using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

internal sealed class ProductSearchService : IProductSearchService
{
    private const int MaxPageSize = 100;

    private readonly ProductManagementDbContext _dbContext;

    public ProductSearchService(ProductManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<ProductListItemDto>> SearchAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        // The query is only described here; nothing runs until CountAsync / ToListAsync.
        var query = _dbContext.Products.AsNoTracking();

        if (request.ActiveOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        if (request.ProductGroupId.HasValue)
        {
            query = query.Where(p => p.ProductGroupId == request.ProductGroupId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(p => p.Name.Contains(term) || p.Sku.Contains(term));
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        query = request.Sort switch
        {
            ProductSort.PriceAscending => query.OrderBy(p => p.Price).ThenBy(p => p.Name),
            ProductSort.PriceDescending => query.OrderByDescending(p => p.Price).ThenBy(p => p.Name),
            ProductSort.StockAscending => query.OrderBy(p => p.StockQuantity).ThenBy(p => p.Name),
            _ => query.OrderBy(p => p.Name),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return new PagedResultDto<ProductListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
