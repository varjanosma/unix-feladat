using ProductManagement.Transfer;

namespace ProductManagement.Bll.Services;

public interface IPriceService
{
    /// <summary>The product price (stored in EUR) converted to <paramref name="currency"/>. <c>null</c> if there is no such product.</summary>
    /// <exception cref="Exceptions.UnknownCurrencyException">The currency does not exist.</exception>
    /// <exception cref="Exceptions.ExchangeRateUnavailableException">The rate service failed or timed out.</exception>
    Task<ProductPriceDto?> GetPriceAsync(int productId, string currency, CancellationToken cancellationToken = default);
}
