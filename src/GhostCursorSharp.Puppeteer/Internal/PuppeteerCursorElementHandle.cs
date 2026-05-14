using PuppeteerSharp;

namespace GhostCursorSharp.Internal;

internal sealed class PuppeteerCursorElementHandle : INativeCursorElementHandle<IElementHandle>
{
    public PuppeteerCursorElementHandle(IElementHandle nativeElement)
    {
        NativeElement = nativeElement;
    }

    public IElementHandle NativeElement { get; }

    public async Task<ElementBox?> BoundingBoxAsync()
    {
        var boundingBox = await NativeElement.BoundingBoxAsync();
        if (boundingBox is null)
        {
            return null;
        }

        return new ElementBox(
            Convert.ToDouble(boundingBox.X),
            Convert.ToDouble(boundingBox.Y),
            Convert.ToDouble(boundingBox.Width),
            Convert.ToDouble(boundingBox.Height));
    }

    public Task EvaluateFunctionAsync(string script, params object?[] args)
        => NativeElement.EvaluateFunctionAsync(script, args);

    public Task<T> EvaluateFunctionAsync<T>(string script, params object?[] args)
        => NativeElement.EvaluateFunctionAsync<T>(script, args);
}
