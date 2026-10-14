namespace Lecture2Lab2.Rates;

public interface IExchangeRateProvider
{
    Task<ExchangeRates> GetRatesAsync(string baseCurrency, IReadOnlyList<string> targets, CancellationToken ct = default);
}
