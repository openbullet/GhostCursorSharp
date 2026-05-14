namespace GhostCursorSharp.Tests;

public enum BrowserTestCase
{
    PuppeteerChromium,
    PlaywrightChromium,
    PlaywrightFirefox
}

public static class BrowserTestCases
{
    public static TheoryData<BrowserTestCase> All
        => new()
        {
            BrowserTestCase.PuppeteerChromium,
            BrowserTestCase.PlaywrightChromium,
            BrowserTestCase.PlaywrightFirefox
        };
}

internal sealed record CursorBox(double X, double Y, double Width, double Height);

internal interface ITestCursor
{
    DefaultOptions? DefaultOptions { get; set; }

    Vector Location { get; }

    Vector GetLocation();

    Task<object> GetElementAsync(string selector, GetElementOptions? options = null);

    Task<object> GetElementAsync(object element, GetElementOptions? options = null);

    Task<CursorBox> GetElementBoxAsync(object element, bool relativeToMainFrame = true);

    Task MoveAsync(string selector, MoveOptions? options = null);

    Task MoveAsync(object element, MoveOptions? options = null);

    Task ClickAsync(string selector, ClickOptions? options = null);

    Task ClickAsync(object element, ClickOptions? options = null);

    Task ScrollAsync(Vector delta, ScrollOptions? options = null);

    Task ScrollToAsync(string destination, ScrollOptions? options = null);

    Task ScrollToAsync(ScrollToDestination destination, ScrollOptions? options = null);

    void ToggleRandomMove(bool random);
}

internal interface IBrowserTestSession : IAsyncDisposable
{
    Task LoadFixtureAsync();

    Task LoadContentAsync(string html);

    Task<object?> QuerySelectorAsync(string selector);

    Task<object> QuerySelectorInFrameAsync(string frameSelector, string selector);

    Task<bool> IsIntersectingViewportAsync(object element, double threshold);

    Task<T> EvaluateExpressionAsync<T>(string script);

    Task<T> EvaluateFunctionAsync<T>(string script, object? arg = null);

    ITestCursor CreateCursor(GhostCursorOptions? options = null);
}
