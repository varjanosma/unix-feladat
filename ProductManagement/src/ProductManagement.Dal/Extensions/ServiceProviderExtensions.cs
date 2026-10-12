using Microsoft.Extensions.DependencyInjection;

namespace ProductManagement.Dal.Extensions;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Creates the database and applies the seed data declared with HasData.
    /// With the in-memory provider this runs on every start, because the database
    /// only lives as long as the process.
    /// </summary>
    public static void InitializeDal(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductManagementDbContext>();
        dbContext.Database.EnsureCreated();
    }
}
