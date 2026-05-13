using PuppeteerSharp;
namespace GhostCursorSharp.Internal;

internal sealed class PuppeteerCursorPageAdapter : ICursorPageAdapter
{
    private readonly IPage _page;

    public PuppeteerCursorPageAdapter(IPage page)
    {
        _page = page;
    }

    public Task WaitForSelectorAsync(string selector, int timeoutMilliseconds)
        => _page.WaitForSelectorAsync(selector, new WaitForSelectorOptions
        {
            Timeout = timeoutMilliseconds
        });

    public async Task<ICursorElementHandle?> QuerySelectorAsync(string selector)
    {
        var element = await _page.QuerySelectorAsync(selector);
        return element is null ? null : new PuppeteerCursorElementHandle(element);
    }

    public async Task<IReadOnlyList<ICursorElementHandle>> QuerySelectorAllAsync(string selector)
        => (await _page.QuerySelectorAllAsync(selector))
            .Select(static element => (ICursorElementHandle)new PuppeteerCursorElementHandle(element))
            .ToArray();

    public Task MoveMouseAsync(double x, double y, int steps = 1)
        => _page.Mouse.MoveAsync(
            Convert.ToDecimal(x),
            Convert.ToDecimal(y),
            new PuppeteerSharp.Input.MoveOptions { Steps = steps });

    public Task MouseWheelAsync(double deltaX, double deltaY)
        => _page.Mouse.WheelAsync(Convert.ToDecimal(deltaX), Convert.ToDecimal(deltaY));

    public Task MouseDownAsync(PuppeteerSharp.Input.MouseButton button, int clickCount)
        => _page.Mouse.DownAsync(new PuppeteerSharp.Input.ClickOptions
        {
            Button = button,
            Count = clickCount
        });

    public Task MouseUpAsync(PuppeteerSharp.Input.MouseButton button, int clickCount)
        => _page.Mouse.UpAsync(new PuppeteerSharp.Input.ClickOptions
        {
            Button = button,
            Count = clickCount
        });

    public Task<T> EvaluateFunctionAsync<T>(string script, params object?[] args)
        => _page.EvaluateFunctionAsync<T>(script, args);

    public Task EvaluateFunctionAsync(string script, params object?[] args)
        => _page.EvaluateFunctionAsync(script, args);
}
