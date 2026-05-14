using System.Text.Json;
using OpenQA.Selenium;

namespace GhostCursorSharp.Internal;

internal static class SeleniumScriptExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static Task<T> EvaluateExpressionAsync<T>(IJavaScriptExecutor executor, string script, params object?[] args)
        => Task.FromResult(ConvertResult<T>(executor.ExecuteScript(script, args)));

    public static Task EvaluateExpressionAsync(IJavaScriptExecutor executor, string script, params object?[] args)
    {
        executor.ExecuteScript(script, args);
        return Task.CompletedTask;
    }

    public static Task<T> EvaluateFunctionAsync<T>(IJavaScriptExecutor executor, string script, params object?[] args)
        => Task.FromResult(ConvertResult<T>(executor.ExecuteScript(
            $"return ({script}).apply(null, arguments);",
            args)));

    public static Task EvaluateFunctionAsync(IJavaScriptExecutor executor, string script, params object?[] args)
    {
        executor.ExecuteScript($"return ({script}).apply(null, arguments);", args);
        return Task.CompletedTask;
    }

    public static T ConvertResult<T>(object? result)
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
