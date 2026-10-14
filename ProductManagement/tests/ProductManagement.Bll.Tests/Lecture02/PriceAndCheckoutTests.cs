using Microsoft.EntityFrameworkCore;
using ProductManagement.Bll.Exceptions;
using ProductManagement.Bll.Services;
using ProductManagement.Transfer;
using Xunit;

namespace ProductManagement.Bll.Tests.Lecture02;

public class PriceAndCheckoutTests
{
    [Fact]
    public async Task Price_is_converted_with_the_rate_of_the_provider()
    {
        using var db = TestDb.Create();
        var service = new PriceService(db, new FakeExchangeRateProvider());

        var price = await service.GetPriceAsync(productId: 6, currency: "huf");   // oil filter, 12.90 EUR

        Assert.Equal("HUF", price!.Currency);
        Assert.Equal(5_160m, price.Price);
    }

    [Fact]
    public async Task Eur_needs_no_rate_service()
    {
        using var db = TestDb.Create();
        var rates = new FakeExchangeRateProvider { IsDown = true };

        var price = await new PriceService(db, rates).GetPriceAsync(productId: 6, currency: "EUR");

        Assert.Equal(12.90m, price!.Price);
        Assert.Equal(0, rates.CallCount);
    }

    [Fact]
    public async Task Unknown_product_is_null_not_an_exception()
    {
        using var db = TestDb.Create();

        Assert.Null(await new PriceService(db, new FakeExchangeRateProvider()).GetPriceAsync(999, "HUF"));
    }

    [Fact]
    public async Task Checkout_places_no_order_when_the_rate_service_is_down()
    {
        using var db = TestDb.Create();
        var checkout = new CheckoutService(new OrderService(db), new PurchaseService(db), new FakeExchangeRateProvider { IsDown = true });
        var purchasesBefore = await db.Purchases.CountAsync();

        var request = new PlaceOrderRequest { UserId = 1, Lines = new[] { new OrderLineRequest { Sku = "ENG-001", Quantity = 1 } } };
        await Assert.ThrowsAsync<ExchangeRateUnavailableException>(() => checkout.CheckoutAsync(request, "HUF"));

        Assert.Equal(purchasesBefore, await db.Purchases.CountAsync());
    }

    [Fact]
    public async Task Checkout_returns_the_total_in_the_requested_currency()
    {
        using var db = TestDb.Create();
        var checkout = new CheckoutService(new OrderService(db), new PurchaseService(db), new FakeExchangeRateProvider());

        var request = new PlaceOrderRequest { UserId = 1, Lines = new[] { new OrderLineRequest { Sku = "BRK-002", Quantity = 2 } } };
        var result = await checkout.CheckoutAsync(request, "USD");

        Assert.Equal(93.00m, result.TotalEur);
        Assert.Equal(102.30m, result.Total);
        Assert.Equal($"/api/export/invoices/{result.PurchaseId}", result.InvoiceUrl);
    }
}
