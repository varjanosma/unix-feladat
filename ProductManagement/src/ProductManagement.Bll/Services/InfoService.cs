using ProductManagement.Dal;

namespace ProductManagement.Bll.Services;

internal sealed class InfoService : IInfoService
{
    private readonly ProductManagementDbContext _dbContext;

    public InfoService(ProductManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string GetBllInfo()
        => $"Bll OK - DbContext: {_dbContext.GetType().Name}, provider: {_dbContext.Database.ProviderName}";
}
