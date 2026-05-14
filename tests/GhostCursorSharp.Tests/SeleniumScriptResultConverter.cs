using System.Text.Json;

namespace GhostCursorSharp.Tests;

internal static class SeleniumScriptResultConverter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static T Convert<T>(object? result)
    {
        if (result is null)
        {
            return default!;
        }

        if (result is T typed)
        {
            return typed;
        }

        if (typeof(T) == typeof(JsonElement))
        {
            var json = JsonSerializer.SerializeToElement(result, JsonOptions);
            return (T)(object)json;
        }

        var serialized = JsonSerializer.Serialize(result, JsonOptions);
        return JsonSerializer.Deserialize<T>(serialized, JsonOptions)!;
    }
}
