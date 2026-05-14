using PlaywrightBoundingBox = Microsoft.Playwright.ElementHandleBoundingBoxResult;
using PlaywrightElementHandle = Microsoft.Playwright.IElementHandle;

namespace GhostCursorSharp.Internal;

internal sealed class PlaywrightCursorElementHandle : INativeCursorElementHandle<PlaywrightElementHandle>
{
    public PlaywrightCursorElementHandle(PlaywrightElementHandle nativeElement)
    {
        NativeElement = nativeElement;
    }

    public PlaywrightElementHandle NativeElement { get; }

    public async Task<ElementBox?> BoundingBoxAsync()
    {
        PlaywrightBoundingBox? boundingBox = await NativeElement.BoundingBoxAsync();
        if (boundingBox is null)
        {
            return null;
        }

        return new ElementBox(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
    }

    public Task EvaluateFunctionAsync(string script, params object?[] args)
        => NativeElement.EvaluateAsync(PlaywrightScriptExecutor.WrapFunction(script), args);

    public Task<T> EvaluateFunctionAsync<T>(string script, params object?[] args)
        => NativeElement.EvaluateAsync<T>(PlaywrightScriptExecutor.WrapFunction(script), args);
}
