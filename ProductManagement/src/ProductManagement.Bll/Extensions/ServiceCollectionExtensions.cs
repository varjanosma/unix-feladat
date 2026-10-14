using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Bll.Rates;
using ProductManagement.Bll.Services;
using ProductManagement.Dal.Extensions;

namespace ProductManagement.Bll.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registration of the Bll layer. Bll pulls in Dal underneath it, so the Web
    /// project never has to know that Dal exists.
    /// </summary>
    public static IServiceCollection AddBll(this IServiceCollection services, string? connectionString = null)
    {
        services.AddDal(connectionString);

        // Scoped: one instance per HTTP request, same lifetime as the DbContext.
        services.AddScoped<IInfoService, InfoService>();
        services.AddScoped<IProductGroupService, ProductGroupService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPurchaseService, PurchaseService>();

        // Lecture 2
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IProductSearchService, ProductSearchService>();
        services.AddScoped<ISalesStatisticsService, SalesStatisticsService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPriceService, PriceService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<ICheckoutService, CheckoutService>();

        // Singleton: one HttpClient for the whole application. Creating one per call
        // exhausts the sockets under load.
        services.AddSingleton<IExchangeRateProvider>(_ => new FrankfurterRateProvider(
            new HttpClient { BaseAddress = new Uri("https://api.frankfurter.dev/v1/") }));

        return services;
    }
}
