using PuppeteerSharp;

namespace GhostCursorSharp.Internal;

internal interface ICursorElementHandle
{
    Task<BoundingBox?> BoundingBoxAsync();

    Task EvaluateFunctionAsync(string script, params object?[] args);

    Task<T> EvaluateFunctionAsync<T>(string script, params object?[] args);
}

internal interface INativeCursorElementHandle<out TNative> : ICursorElementHandle
{
    TNative NativeElement { get; }
}
