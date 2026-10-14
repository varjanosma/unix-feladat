using System.Text.Json;
using Lecture2Lab2;
using Lecture2Lab2.Models;
using Lecture2Lab2.Processing;
using Lecture2Lab2.Rates;

// Usage: dotnet run            -> live rates over HTTP
//        dotnet run -- offline -> fixed rates, no network

using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));   // a timeout is just a cancellation
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

using var http = new HttpClient { BaseAddress = new Uri("https://api.frankfurter.dev/v1/") };
IExchangeRateProvider rateProvider = args.Contains("offline")
    ? new FixedRateProvider()
    : new FrankfurterRateProvider(http);

try
{
    List<PriceItem> items;
    await using (var input = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "prices.json")))
    {
        items = await JsonSerializer.DeserializeAsync<List<PriceItem>>(input, JsonDefaults.Options, cts.Token)
                ?? new List<PriceItem>();
    }

    var rates = await rateProvider.GetRatesAsync("HUF", new[] { "EUR", "USD" }, cts.Token);
    var priceList = PriceConverter.Convert(items, rates);

    await using (var output = File.Create("pricelist.json"))
    {
        await JsonSerializer.SerializeAsync(output, priceList, JsonDefaults.Options, cts.Token);
    }

    Console.WriteLine($"{priceList.Items.Count} prices converted with rates of {priceList.RateDate} -> pricelist.json");
}
catch (OperationCanceledException)
{
    // The top level is the right place to turn failures into a message and an exit code.
    Console.Error.WriteLine("Cancelled: the rate service did not answer within 5 seconds.");
    return 2;
}
catch (ExchangeRateUnavailableException ex)
{
    Console.Error.WriteLine($"{ex.Message} ({ex.InnerException?.Message})");
    return 1;
}

return 0;
