using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

public interface ICheckoutService
{
    /// <summary>Places the order and returns its total in <paramref name="currency"/>.</summary>
    /// <exception cref="Exceptions.OrderValidationException">The request is invalid.</exception>
    /// <exception cref="Exceptions.InsufficientStockException">A product does not have enough stock.</exception>
    /// <exception cref="Exceptions.UnknownCurrencyException">The currency does not exist.</exception>
    /// <exception cref="Exceptions.ExchangeRateUnavailableException">The rate service failed or timed out.</exception>
    Task<CheckoutResultDto> CheckoutAsync(PlaceOrderRequest request, string currency, CancellationToken cancellationToken = default);
}
