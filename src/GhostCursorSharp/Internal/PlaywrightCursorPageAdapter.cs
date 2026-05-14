using Microsoft.Playwright;
using PuppeteerMouseButton = PuppeteerSharp.Input.MouseButton;

namespace GhostCursorSharp.Internal;

internal sealed class PlaywrightCursorPageAdapter : ICursorPageAdapter
{
    private readonly IPage _page;

    public PlaywrightCursorPageAdapter(IPage page)
    {
        _page = page;
    }

    public string CreateXPathSelector(string selector)
        => $"xpath={selector}";

    public async Task WaitForSelectorAsync(string selector, int timeoutMilliseconds)
    {
        await _page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            Timeout = timeoutMilliseconds
        });
    }

    public async Task<ICursorElementHandle?> QuerySelectorAsync(string selector)
    {
        var element = await _page.QuerySelectorAsync(selector);
        return element is null ? null : new PlaywrightCursorElementHandle(element);
    }

    public async Task<IReadOnlyList<ICursorElementHandle>> QuerySelectorAllAsync(string selector)
        => (await _page.QuerySelectorAllAsync(selector))
            .Select(static element => (ICursorElementHandle)new PlaywrightCursorElementHandle(element))
            .ToArray();

    public Task MoveMouseAsync(double x, double y, int steps = 1)
        => _page.Mouse.MoveAsync((float)x, (float)y, new MouseMoveOptions
        {
            Steps = steps
        });

    public Task MouseWheelAsync(double deltaX, double deltaY)
        => _page.Mouse.WheelAsync((float)deltaX, (float)deltaY);

    public Task MouseDownAsync(PuppeteerMouseButton button, int clickCount)
        => _page.Mouse.DownAsync(new MouseDownOptions
        {
            Button = MapButton(button),
            ClickCount = clickCount
        });

    public Task MouseUpAsync(PuppeteerMouseButton button, int clickCount)
        => _page.Mouse.UpAsync(new MouseUpOptions
        {
            Button = MapButton(button),
            ClickCount = clickCount
        });

    public Task<T> EvaluateFunctionAsync<T>(string script, params object?[] args)
        => _page.EvaluateAsync<T>(PlaywrightScriptExecutor.WrapFunction(script), args);

    public Task EvaluateFunctionAsync(string script, params object?[] args)
        => _page.EvaluateAsync(PlaywrightScriptExecutor.WrapFunction(script), args);

    private static Microsoft.Playwright.MouseButton MapButton(PuppeteerMouseButton button)
        => button switch
        {
            PuppeteerMouseButton.Left => Microsoft.Playwright.MouseButton.Left,
            PuppeteerMouseButton.Middle => Microsoft.Playwright.MouseButton.Middle,
            PuppeteerMouseButton.Right => Microsoft.Playwright.MouseButton.Right,
            _ => throw new NotSupportedException($"Mouse button '{button}' is not supported by Playwright.")
        };
}
