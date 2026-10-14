using ProductManagement.Bll.Services;
using ProductManagement.Transfer;
using Xunit;

namespace ProductManagement.Bll.Tests.Lecture02;

public class ProductSearchAndStatisticsTests
{
    [Fact]
    public async Task Search_sorts_and_pages()
    {
        using var db = TestDb.Create();
        var service = new ProductSearchService(db);

        var page3 = await service.SearchAsync(new ProductSearchRequest { Sort = ProductSort.PriceDescending, Page = 3, PageSize = 5 });

        Assert.Equal(11, page3.TotalCount);        // 12 products, 1 inactive
        Assert.Equal(3, page3.TotalPages);
        Assert.Equal("ENG-001", Assert.Single(page3.Items).Sku);   // the cheapest one is last
    }

    [Fact]
    public async Task Search_filters_by_price_range()
    {
        using var db = TestDb.Create();

        var result = await new ProductSearchService(db).SearchAsync(new ProductSearchRequest { MinPrice = 100m, MaxPrice = 500m });

        Assert.All(result.Items, p => Assert.InRange(p.Price, 100m, 500m));
        Assert.Equal(3, result.TotalCount);   // TYR-001, TYR-003, BRK-001 (the roof box is inactive)
    }

    [Fact]
    public async Task Top_products_are_ordered_by_quantity_then_revenue()
    {
        using var db = TestDb.Create();

        var top = await new SalesStatisticsService(db).GetTopProductsAsync(count: 2);

        // Both sold 4 pieces; TYR-001 brought more money.
        Assert.Equal(new[] { "TYR-001", "TYR-002" }, top.Select(t => t.Sku));
        Assert.Equal(436m, top[0].Revenue);
    }

    [Fact]
    public async Task Revenue_by_group_starts_with_vehicles()
    {
        using var db = TestDb.Create();

        var groups = await new SalesStatisticsService(db).GetRevenueByGroupAsync();

        Assert.Equal("Vehicles", groups[0].ProductGroupName);
        Assert.Equal(18_900m, groups[0].Revenue);
    }
}
