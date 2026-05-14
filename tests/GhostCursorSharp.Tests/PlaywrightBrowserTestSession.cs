using Microsoft.Playwright;

namespace GhostCursorSharp.Tests;

internal sealed class PlaywrightBrowserTestSession : IBrowserTestSession
{
    private static readonly string FixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "custom-page.html");

    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private readonly IPage _page;

    private PlaywrightBrowserTestSession(IPlaywright playwright, IBrowser browser, IPage page)
    {
        _playwright = playwright;
        _browser = browser;
        _page = page;
    }

    public static async Task<PlaywrightBrowserTestSession> CreateAsync(BrowserTestCase browserTestCase)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        try
        {
            var browser = await LaunchBrowserAsync(playwright, browserTestCase);
            var page = await browser.NewPageAsync();
            return new PlaywrightBrowserTestSession(playwright, browser, page);
        }
        catch (PlaywrightException ex) when (NeedsBrowserInstall(ex))
        {
            var browserName = browserTestCase == BrowserTestCase.PlaywrightFirefox ? "firefox" : "chromium";
            var exitCode = Microsoft.Playwright.Program.Main(["install", browserName]);
            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Playwright browser installation failed for '{browserName}' with exit code {exitCode}.",
                    ex);
            }

            var browser = await LaunchBrowserAsync(playwright, browserTestCase);
            var page = await browser.NewPageAsync();
            return new PlaywrightBrowserTestSession(playwright, browser, page);
        }
    }

    public async Task LoadFixtureAsync()
    {
        var html = await File.ReadAllTextAsync(FixturePath);
        await LoadContentAsync(html);
    }

    public async Task LoadContentAsync(string html)
    {
        await _page.SetViewportSizeAsync(800, 600);
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

    public Task<bool> IsIntersectingViewportAsync(object element, double threshold)
        => ((IElementHandle)element).EvaluateAsync<bool>(
            """
            (el, threshold) => {
              const rect = el.getBoundingClientRect();
              return rect.bottom > threshold &&
                rect.right > threshold &&
                rect.top < window.innerHeight - threshold &&
                rect.left < window.innerWidth - threshold;
            }
            """,
            threshold);

    public Task<T> EvaluateExpressionAsync<T>(string script)
        => _page.EvaluateAsync<T>(script);

    public Task<T> EvaluateFunctionAsync<T>(string script, object? arg = null)
        => arg is null
            ? _page.EvaluateAsync<T>(script)
            : _page.EvaluateAsync<T>($"(arg) => ({script})(arg)", arg);

    public ITestCursor CreateCursor(GhostCursorOptions? options = null)
        => new PlaywrightTestCursor(options is null ? new PlaywrightGhostCursor(_page) : new PlaywrightGhostCursor(_page, options));

    public async ValueTask DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    private static async Task<IBrowser> LaunchBrowserAsync(IPlaywright playwright, BrowserTestCase browserTestCase)
        => browserTestCase switch
        {
            BrowserTestCase.PlaywrightChromium => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            }),
            BrowserTestCase.PlaywrightFirefox => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(browserTestCase), browserTestCase, "Unsupported Playwright browser test case.")
        };

    private static bool NeedsBrowserInstall(PlaywrightException exception)
        => exception.Message.Contains("Executable doesn't exist", StringComparison.OrdinalIgnoreCase);

    private sealed class PlaywrightTestCursor : ITestCursor
    {
        private readonly PlaywrightGhostCursor _cursor;

        public PlaywrightTestCursor(PlaywrightGhostCursor cursor)
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
            return new CursorBox(box.X, box.Y, box.Width, box.Height);
        }

        public Task MoveAsync(string selector, MoveOptions? options = null)
            => _cursor.MoveAsync(selector, options);

        public Task MoveAsync(object element, MoveOptions? options = null)
            => _cursor.MoveAsync((IElementHandle)element, options);

        public Task ClickAsync(string selector, ClickOptions? options = null)
            => _cursor.ClickAsync(selector, options);

        public Task ClickAsync(object element, ClickOptions? options = null)
            => _cursor.ClickAsync((IElementHandle)element, options);

        public Task InstallMouseHelperAsync()
            => _cursor.InstallMouseHelperAsync();

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
