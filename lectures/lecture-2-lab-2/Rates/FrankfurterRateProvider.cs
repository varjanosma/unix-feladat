using System.Net.Http.Json;
using System.Text.Json;

namespace Lecture2Lab2.Rates;

/// <summary>Live rates from https://frankfurter.dev (ECB data, no API key).</summary>
internal sealed class FrankfurterRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _http;

    // The HttpClient is passed in, not created here: in ASP.NET Core it comes from IHttpClientFactory.
    public FrankfurterRateProvider(HttpClient http) => _http = http;

    public async Task<ExchangeRates> GetRatesAsync(
        string baseCurrency, IReadOnlyList<string> targets, CancellationToken ct = default)
    {
        var url = $"latest?base={baseCurrency}&symbols={string.Join(',', targets)}";

        try
        {
            // Throws HttpRequestException on a non-success status code (404, 500, ...).
            return await _http.GetFromJsonAsync<ExchangeRates>(url, JsonDefaults.Options, ct)
                   ?? throw new ExchangeRateUnavailableException("The rate service returned an empty response.");
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            throw new ExchangeRateUnavailableException($"Could not get rates from '{_http.BaseAddress}{url}'.", ex);
        }
        // OperationCanceledException (timeout / Ctrl+C) is deliberately NOT caught here:
        // cancelling is the caller's decision, so the caller handles it.
    }
}
