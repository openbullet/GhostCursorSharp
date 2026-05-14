using PuppeteerSharp;
using System.Runtime.InteropServices;

namespace GhostCursorSharp.Tests;

internal sealed class PuppeteerBrowserTestSession : IBrowserTestSession
{
    private static readonly string FixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "custom-page.html");
    private static readonly string BrowserCachePath = Environment.GetEnvironmentVariable("GHOSTCURSORSHARP_BROWSER_CACHE_PATH")
        ?? Path.Combine(AppContext.BaseDirectory, ".browser");

    private readonly IBrowser _browser;
    private readonly IPage _page;

    private PuppeteerBrowserTestSession(IBrowser browser, IPage page)
    {
        _browser = browser;
        _page = page;
    }

    public static async Task<PuppeteerBrowserTestSession> CreateAsync()
    {
        var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
        {
            Path = BrowserCachePath
        });

        var installedBrowser = browserFetcher.GetInstalledBrowsers().FirstOrDefault()
            ?? await browserFetcher.DownloadAsync();

        string[] launchArguments = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? ["--no-sandbox", "--disable-setuid-sandbox"]
            : [];

        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = installedBrowser.GetExecutablePath(),
            DefaultViewport = null,
            Args = launchArguments
        });

        var page = await browser.NewPageAsync();
        return new PuppeteerBrowserTestSession(browser, page);
    }

    public async Task LoadFixtureAsync()
    {
        var html = await File.ReadAllTextAsync(FixturePath);
        await LoadContentAsync(html);
    }

    public async Task LoadContentAsync(string html)
    {
        await _page.SetViewportAsync(new ViewPortOptions
        {
            Width = 800,
            Height = 600
        });

        await _page.SetContentAsync(html);
    }

    public Task<object?> QuerySelectorAsync(string selector)
        => _page.QuerySelectorAsync(selector).ContinueWith(static task => (object?)task.Result);

    public async Task<object> QuerySelectorInFrameAsync(string frameSelector, string selector)
    {
        var frameElement = await _page.QuerySelectorAsync(frameSelector)
            ?? throw new InvalidOperationException($"Frame '{frameSelector}' was not found.");
        var frame = await frameElement.ContentFrameAsync()
            ?? throw new InvalidOperationException($"Content frame for '{frameSelector}' was not available.");
        return await frame.QuerySelectorAsync(selector)
            ?? throw new InvalidOperationException($"Frame selector '{selector}' was not found.");
    }

    public async Task<bool> IsIntersectingViewportAsync(object element, double threshold)
        => await ((IElementHandle)element).IsIntersectingViewportAsync(Convert.ToDecimal(threshold));

    public Task<T> EvaluateExpressionAsync<T>(string script)
        => _page.EvaluateExpressionAsync<T>(script);

    public Task<T> EvaluateFunctionAsync<T>(string script, object? arg = null)
        => arg is null
            ? _page.EvaluateFunctionAsync<T>(script)
            : _page.EvaluateFunctionAsync<T>(script, arg);

    public ITestCursor CreateCursor(GhostCursorOptions? options = null)
        => new PuppeteerTestCursor(options is null ? new GhostCursor(_page) : new GhostCursor(_page, options));

    public async ValueTask DisposeAsync()
        => await _browser.CloseAsync();

    private sealed class PuppeteerTestCursor : ITestCursor
    {
        private readonly GhostCursor _cursor;

        public PuppeteerTestCursor(GhostCursor cursor)
        {
            _cursor = cursor;
        }

        public DefaultOptions? DefaultOptions
        {
            get => _cursor.DefaultOptions;
            set => _cursor.DefaultOptions = value;
        }

        public Vector Location => _cursor.Location;

        public Vector GetLocation()
            => _cursor.GetLocation();

        public async Task<object> GetElementAsync(string selector, GetElementOptions? options = null)
            => await _cursor.GetElementAsync(selector, options);

        public async Task<object> GetElementAsync(object element, GetElementOptions? options = null)
            => await _cursor.GetElementAsync((IElementHandle)element, options);

        public async Task<CursorBox> GetElementBoxAsync(object element, bool relativeToMainFrame = true)
        {
            var box = await _cursor.GetElementBoxAsync((IElementHandle)element, relativeToMainFrame);
            return new CursorBox(
                Convert.ToDouble(box.X),
                Convert.ToDouble(box.Y),
                Convert.ToDouble(box.Width),
                Convert.ToDouble(box.Height));
        }

        public Task MoveAsync(string selector, MoveOptions? options = null)
            => _cursor.MoveAsync(selector, options);

        public Task MoveAsync(object element, MoveOptions? options = null)
            => _cursor.MoveAsync((IElementHandle)element, options);

        public Task ClickAsync(string selector, ClickOptions? options = null)
            => _cursor.ClickAsync(selector, options);

        public Task ClickAsync(object element, ClickOptions? options = null)
            => _cursor.ClickAsync((IElementHandle)element, options);

        public Task ScrollAsync(Vector delta, ScrollOptions? options = null)
            => _cursor.ScrollAsync(delta, options);

        public Task ScrollToAsync(string destination, ScrollOptions? options = null)
            => _cursor.ScrollToAsync(destination, options);

        public Task ScrollToAsync(ScrollToDestination destination, ScrollOptions? options = null)
            => _cursor.ScrollToAsync(destination, options);

        public void ToggleRandomMove(bool random)
            => _cursor.ToggleRandomMove(random);
    }
}
