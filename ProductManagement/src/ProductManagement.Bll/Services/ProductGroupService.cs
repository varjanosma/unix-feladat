using Microsoft.EntityFrameworkCore;
using ProductManagement.Transfer;
using ProductManagement.Dal;

namespace ProductManagement.Bll.Services;

internal sealed class ProductGroupService : IProductGroupService
{
    private readonly ProductManagementDbContext _dbContext;

    public ProductGroupService(ProductManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductGroupDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // AsNoTracking: read-only query, EF does not need to track the entities.
        // The Select runs on the database side and only fetches what the DTO needs.
        return await _dbContext.ProductGroups
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new ProductGroupDto
            {
                Id = g.Id,
                Code = g.Code,
                Name = g.Name,
                Description = g.Description,
                ProductCount = g.Products.Count,
            })
            .ToListAsync(cancellationToken);
    }
}
