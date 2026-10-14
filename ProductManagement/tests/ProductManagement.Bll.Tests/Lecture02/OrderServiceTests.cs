using Microsoft.EntityFrameworkCore;
using ProductManagement.Bll.Exceptions;
using ProductManagement.Bll.Services;
using ProductManagement.Transfer;
using Xunit;

namespace ProductManagement.Bll.Tests.Lecture02;

public class OrderServiceTests
{
    private static PlaceOrderRequest Order(int userId, params (string Sku, int Quantity)[] lines) => new()
    {
        UserId = userId,
        Lines = lines.Select(l => new OrderLineRequest { Sku = l.Sku, Quantity = l.Quantity }).ToList(),
    };

    [Fact]
    public async Task A_valid_order_creates_a_purchase_and_decreases_the_stock()
    {
        using var db = TestDb.Create();
        var service = new OrderService(db);

        var purchaseId = await service.PlaceOrderAsync(Order(2, ("tyr-001", 4), ("ENG-001", 2)));

        var purchase = await db.Purchases.Include(p => p.Items).SingleAsync(p => p.Id == purchaseId);
        Assert.Equal(2, purchase.Items.Count);
        Assert.Equal(60, (await db.Products.SingleAsync(p => p.Sku == "TYR-001")).StockQuantity);   // 64 - 4
    }

    [Fact]
    public async Task All_validation_errors_are_reported_together()
    {
        using var db = TestDb.Create();

        var ex = await Assert.ThrowsAsync<OrderValidationException>(() =>
            new OrderService(db).PlaceOrderAsync(Order(999, ("NOPE-1", 1), ("TYR-001", 0), ("ACC-002", 1))));

        Assert.Equal(4, ex.Errors.Count);   // unknown user, unknown SKU, zero quantity, inactive product
    }

    [Fact]
    public async Task Not_enough_stock_throws_and_changes_nothing()
    {
        using var db = TestDb.Create();
        var purchasesBefore = await db.Purchases.CountAsync();

        var ex = await Assert.ThrowsAsync<InsufficientStockException>(() =>
            new OrderService(db).PlaceOrderAsync(Order(1, ("TYR-001", 1), ("VEH-002", 5))));

        Assert.Equal(("VEH-002", 5, 2), (ex.Sku, ex.Requested, ex.Available));
        Assert.Equal(purchasesBefore, await db.Purchases.CountAsync());
        Assert.Equal(64, (await db.Products.SingleAsync(p => p.Sku == "TYR-001")).StockQuantity);
    }
}
