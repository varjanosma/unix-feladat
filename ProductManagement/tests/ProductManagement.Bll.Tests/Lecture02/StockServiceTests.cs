using ProductManagement.Bll.Services;
using Xunit;

namespace ProductManagement.Bll.Tests.Lecture02;

public class StockServiceTests
{
    [Fact]
    public async Task Low_stock_is_grouped_and_skips_inactive_products()
    {
        using var db = TestDb.Create();
        var service = new StockService(db);

        var lowStock = await service.GetLowStockByGroupAsync(threshold: 10);

        Assert.Equal(new[] { "VEH-002", "VEH-001" }, lowStock["Vehicles"].Select(i => i.Sku));
        Assert.Equal("ENG-003", Assert.Single(lowStock["Engine and service"]).Sku);
        Assert.DoesNotContain(lowStock.Values.SelectMany(v => v), i => i.Sku == "ACC-002");   // inactive
    }

    [Fact]
    public async Task Recommendations_are_unbought_products_from_the_same_groups()
    {
        using var db = TestDb.Create();
        var service = new StockService(db);

        // User 1 bought TYR-001, ENG-001, ENG-002 -> groups "tyres" and "engine".
        var recommended = await service.GetRecommendationsAsync(userId: 1);

        // ENG-003 is in the right group but out of stock.
        Assert.Equal(new[] { "TYR-003", "TYR-002" }, recommended!.Select(p => p.Sku));
    }

    [Fact]
    public async Task Recommendations_for_an_unknown_user_are_null()
    {
        using var db = TestDb.Create();

        Assert.Null(await new StockService(db).GetRecommendationsAsync(userId: 999));
    }
}
