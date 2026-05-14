using PlaywrightBoundingBox = Microsoft.Playwright.ElementHandleBoundingBoxResult;
using PlaywrightElementHandle = Microsoft.Playwright.IElementHandle;
using PuppeteerSharp;

namespace GhostCursorSharp.Internal;

internal sealed class PlaywrightCursorElementHandle : INativeCursorElementHandle<PlaywrightElementHandle>
{
    public PlaywrightCursorElementHandle(PlaywrightElementHandle nativeElement)
    {
        NativeElement = nativeElement;
    }

    public PlaywrightElementHandle NativeElement { get; }

    public async Task<BoundingBox?> BoundingBoxAsync()
    {
        PlaywrightBoundingBox? boundingBox = await NativeElement.BoundingBoxAsync();
        if (boundingBox is null)
        {
            return null;
        }

        return new BoundingBox
        {
            X = Convert.ToDecimal(boundingBox.X),
            Y = Convert.ToDecimal(boundingBox.Y),
            Width = Convert.ToDecimal(boundingBox.Width),
            Height = Convert.ToDecimal(boundingBox.Height)
        };
    }

    public Task EvaluateFunctionAsync(string script, params object?[] args)
        => NativeElement.EvaluateAsync(PlaywrightScriptExecutor.WrapFunction(script), args);

    public Task<T> EvaluateFunctionAsync<T>(string script, params object?[] args)
        => NativeElement.EvaluateAsync<T>(PlaywrightScriptExecutor.WrapFunction(script), args);
}
