using ProductManagement.Bll.Rates;
using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

/// <summary>Composes the other services; it only knows them through their interfaces.</summary>
internal sealed class CheckoutService : ICheckoutService
{
    private readonly IOrderService _orderService;
    private readonly IPurchaseService _purchaseService;
    private readonly IExchangeRateProvider _rateProvider;

    public CheckoutService(IOrderService orderService, IPurchaseService purchaseService, IExchangeRateProvider rateProvider)
    {
        _orderService = orderService;
        _purchaseService = purchaseService;
        _rateProvider = rateProvider;
    }

    public async Task<CheckoutResultDto> CheckoutAsync(
        PlaceOrderRequest request,
        string currency,
        CancellationToken cancellationToken = default)
    {
        // The rate first: if the rate service is down, no order is placed at all.
        var rate = await PriceService.GetRateFromEurAsync(_rateProvider, currency, cancellationToken);

        var purchaseId = await _orderService.PlaceOrderAsync(request, cancellationToken);

        var purchase = await _purchaseService.GetByIdAsync(purchaseId, cancellationToken)
                       ?? throw new InvalidOperationException($"Purchase {purchaseId} disappeared right after it was created.");

        return new CheckoutResultDto
        {
            PurchaseId = purchaseId,
            TotalEur = purchase.Total,
            Currency = rate.To,
            Rate = rate.Rate,
            Total = Money.Round(purchase.Total * rate.Rate),
            InvoiceUrl = $"/api/export/invoices/{purchaseId}",
        };
    }
}
