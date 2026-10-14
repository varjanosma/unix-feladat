using ProductManagement.Bll.Exceptions;
using ProductManagement.Bll.Rates;

namespace ProductManagement.Bll.Tests.Lecture02;

/// <summary>Fixed rates, no network. It also counts the calls, so a test can check that none was made.</summary>
internal sealed class FakeExchangeRateProvider : IExchangeRateProvider
{
    private readonly Dictionary<string, decimal> _rates = new() { ["HUF"] = 400m, ["USD"] = 1.10m };

    public bool IsDown { get; init; }

    public int CallCount { get; private set; }

    public Task<ExchangeRate> GetRateAsync(string from, string to, CancellationToken cancellationToken = default)
    {
        CallCount++;

        if (IsDown)
        {
            throw new ExchangeRateUnavailableException("Fake: the rate service is down.");
        }

        return _rates.TryGetValue(to, out var rate)
            ? Task.FromResult(new ExchangeRate(from, to, rate, "2026-01-01"))
            : throw new UnknownCurrencyException(to);
    }
}
