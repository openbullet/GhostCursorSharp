namespace GhostCursorSharp.Internal;

internal interface ICursorElementHandle
{
    Task<ElementBox?> BoundingBoxAsync();

    Task EvaluateFunctionAsync(string script, params object?[] args);

    Task<T> EvaluateFunctionAsync<T>(string script, params object?[] args);
}

internal interface INativeCursorElementHandle<out TNative> : ICursorElementHandle
{
    TNative NativeElement { get; }
}
