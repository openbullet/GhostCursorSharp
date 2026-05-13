using PuppeteerSharp;

namespace GhostCursorSharp.Internal;

internal sealed class PuppeteerCursorElementHandle : INativeCursorElementHandle<IElementHandle>
{
    public PuppeteerCursorElementHandle(IElementHandle nativeElement)
    {
        NativeElement = nativeElement;
    }

    public IElementHandle NativeElement { get; }

    public Task<BoundingBox?> BoundingBoxAsync()
        => NativeElement.BoundingBoxAsync();

    public Task EvaluateFunctionAsync(string script, params object?[] args)
        => NativeElement.EvaluateFunctionAsync(script, args);

    public Task<T> EvaluateFunctionAsync<T>(string script, params object?[] args)
        => NativeElement.EvaluateFunctionAsync<T>(script, args);
}
