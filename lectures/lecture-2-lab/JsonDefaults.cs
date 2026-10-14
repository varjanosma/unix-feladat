using System.Text.Json;

namespace Lecture2Lab;

internal static class JsonDefaults
{
    // Created once and reused: building options is expensive.
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
}
