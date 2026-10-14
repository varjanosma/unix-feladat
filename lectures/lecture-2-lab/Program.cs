using System.Text.Json;
using Lecture2Lab;
using Lecture2Lab.Processing;
using Lecture2Lab.Sources;

// Ctrl+C cancels the I/O instead of killing the process mid-write.
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var inputPath = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "orders.json");

IOrderSource source = new JsonFileOrderSource(inputPath);
var orders = await source.LoadAsync(cts.Token);

var summary = OrderProcessor.Summarize(orders);

await using (var output = File.Create("summary.json"))
{
    await JsonSerializer.SerializeAsync(output, summary, JsonDefaults.Options, cts.Token);
}

Console.WriteLine($"{summary.Totals.Count} SKUs, {summary.Failures.Count} failed orders -> summary.json");
