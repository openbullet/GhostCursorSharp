namespace GhostCursorSharp.Internal;

internal static class PlaywrightScriptExecutor
{
    public static string WrapFunction(string script)
        => $"(arg) => {{ const args = Array.isArray(arg) ? arg : [arg]; return ({script})(...args); }}";
}
