using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProductManagement.Bll.Exceptions;

namespace ProductManagement.Bll.Rates;

/// <summary>Live ECB rates from https://frankfurter.dev (no API key needed).</summary>
internal sealed class FrankfurterRateProvider : IExchangeRateProvider
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private readonly HttpClient _http;

    public FrankfurterRateProvider(HttpClient http)
    {
        _http = http;
    }

    public async Task<ExchangeRate> GetRateAsync(string from, string to, CancellationToken cancellationToken = default)
    {
        var url = $"latest?base={from}&symbols={to}";

        // Our own time limit on top of the caller's token: whichever fires first cancels the call.
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);

        try
        {
            var response = await _http.GetFromJsonAsync<FrankfurterResponse>(url, timeout.Token);

            if (response is null || !response.Rates.TryGetValue(to, out var rate))
            {
                throw new ExchangeRateUnavailableException($"No {from}->{to} rate in the response.");
            }

            return new ExchangeRate(from, to, rate, response.Date);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Our timeout fired, not the caller's token: that is a failure, not a cancellation.
            throw new ExchangeRateUnavailableException($"The rate service did not answer within {Timeout.TotalSeconds} seconds.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UnknownCurrencyException(to);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            throw new ExchangeRateUnavailableException($"Could not get the {from}->{to} rate: {ex.Message}", ex);
        }
    }

    private sealed record FrankfurterResponse(string Base, string Date, Dictionary<string, decimal> Rates);
}
