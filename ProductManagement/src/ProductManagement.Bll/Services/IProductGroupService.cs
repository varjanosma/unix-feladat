using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

public interface IProductGroupService
{
    Task<IReadOnlyList<ProductGroupDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
