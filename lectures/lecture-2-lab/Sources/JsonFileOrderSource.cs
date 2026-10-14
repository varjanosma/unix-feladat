using System.Text.Json;
using Lecture2Lab.Models;

namespace Lecture2Lab.Sources;

internal sealed class JsonFileOrderSource : IOrderSource
{
    private readonly string _path;

    public JsonFileOrderSource(string path) => _path = path;

    public async Task<IReadOnlyList<Order>> LoadAsync(CancellationToken ct = default)
    {
        try
        {
            await using var stream = File.OpenRead(_path);
            return await JsonSerializer.DeserializeAsync<List<Order>>(stream, JsonDefaults.Options, ct)
                   ?? new List<Order>();
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            // Not swallowed: wrapped with the context the caller needs, then rethrown.
            throw new OrderImportException(_path, ex);
        }
    }
}
