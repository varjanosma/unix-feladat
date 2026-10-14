namespace ProductManagement.Bll.Rates;

public interface IExchangeRateProvider
{
    /// <exception cref="Exceptions.UnknownCurrencyException">The currency does not exist.</exception>
    /// <exception cref="Exceptions.ExchangeRateUnavailableException">The rate service failed or timed out.</exception>
    Task<ExchangeRate> GetRateAsync(string from, string to, CancellationToken cancellationToken = default);
}
