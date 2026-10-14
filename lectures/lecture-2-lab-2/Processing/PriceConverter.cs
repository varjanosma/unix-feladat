using Lecture2Lab2.Models;
using Lecture2Lab2.Rates;

namespace Lecture2Lab2.Processing;

public static class PriceConverter
{
    public static PriceList Convert(IEnumerable<PriceItem> items, ExchangeRates rates)
    {
        var eur = RequireRate(rates, "EUR");
        var usd = RequireRate(rates, "USD");

        var converted = items
            .Select(i => new ConvertedPrice(i.Sku, i.Name, i.PriceHuf, Round(i.PriceHuf * eur), Round(i.PriceHuf * usd)))
            .OrderBy(p => p.Sku)
            .ToList();

        return new PriceList(rates.Date, converted);
    }

    private static decimal RequireRate(ExchangeRates rates, string currency) =>
        rates.Rates.TryGetValue(currency, out var rate)
            ? rate
            : throw new ExchangeRateUnavailableException($"No {currency} rate in the response.");

    // decimal + explicit rounding mode: money is never double, and banker's rounding is a surprise.
    private static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
