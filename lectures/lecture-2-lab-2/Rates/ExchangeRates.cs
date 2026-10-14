namespace Lecture2Lab2.Rates;

/// <summary>Matches the API response: { "base": "HUF", "date": "...", "rates": { "EUR": 0.0025 } }.</summary>
public sealed record ExchangeRates(string Base, string Date, Dictionary<string, decimal> Rates);
