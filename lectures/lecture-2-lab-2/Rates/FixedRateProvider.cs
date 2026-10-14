namespace Lecture2Lab2.Rates;

/// <summary>Offline fake: no network, same rates every time. Usable in tests and behind a firewall.</summary>
internal sealed class FixedRateProvider : IExchangeRateProvider
{
    public Task<ExchangeRates> GetRatesAsync(
        string baseCurrency, IReadOnlyList<string> targets, CancellationToken ct = default)
    {
        var rates = new Dictionary<string, decimal> { ["EUR"] = 0.00250m, ["USD"] = 0.00280m };
        return Task.FromResult(new ExchangeRates(baseCurrency, "fixed", rates));
    }
}
