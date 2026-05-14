using OpenQA.Selenium;

namespace GhostCursorSharp.Internal;

internal sealed class SeleniumCursorElementHandle : INativeCursorElementHandle<IWebElement>
{
    private readonly IJavaScriptExecutor _javascriptExecutor;

    public SeleniumCursorElementHandle(IWebDriver driver, IWebElement nativeElement)
    {
        NativeElement = nativeElement;
        _javascriptExecutor = driver as IJavaScriptExecutor
            ?? throw new InvalidOperationException("The Selenium driver must implement IJavaScriptExecutor.");
    }

    public IWebElement NativeElement { get; }

    public Task<ElementBox?> BoundingBoxAsync()
    {
        var location = NativeElement.Location;
        var size = NativeElement.Size;
        return Task.FromResult<ElementBox?>(new ElementBox(location.X, location.Y, size.Width, size.Height));
    }

    public Task EvaluateFunctionAsync(string script, params object?[] args)
        => SeleniumScriptExecutor.EvaluateFunctionAsync(_javascriptExecutor, script, [NativeElement, .. args]);

    public Task<T> EvaluateFunctionAsync<T>(string script, params object?[] args)
        => SeleniumScriptExecutor.EvaluateFunctionAsync<T>(_javascriptExecutor, script, [NativeElement, .. args]);
}
