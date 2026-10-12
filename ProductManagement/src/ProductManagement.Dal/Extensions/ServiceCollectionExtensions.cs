using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ProductManagement.Dal.Extensions;

/// <summary>
/// Registration of the Dal layer. The layer itself decides what goes into the DI
/// container; the caller only needs to know a single method.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDal(this IServiceCollection services, string? connectionString = null)
    {
        // Demo: in-memory provider so the solution starts without a database.
        // For real use: services.AddDbContext<...>(o => o.UseSqlServer(connectionString));
        services.AddDbContext<ProductManagementDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: "ProductManagement"));

        return services;
    }
}
