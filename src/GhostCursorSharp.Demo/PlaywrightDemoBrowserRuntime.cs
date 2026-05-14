using Microsoft.Playwright;

namespace GhostCursorSharp.Demo;

internal sealed class PlaywrightDemoBrowserRuntime : IDemoBrowserRuntime
{
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private readonly IPage _page;
    private readonly PlaywrightDemoCursor _cursor;

    private PlaywrightDemoBrowserRuntime(IPlaywright playwright, IBrowser browser, IPage page)
    {
        _playwright = playwright;
        _browser = browser;
        _page = page;
        _cursor = new PlaywrightDemoCursor(new PlaywrightGhostCursor(page, new Vector(140, 140)));
    }

    public IDemoCursor Cursor => _cursor;

    public static async Task<PlaywrightDemoBrowserRuntime> CreateAsync(DemoBrowserTarget target)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        try
        {
            var browser = await LaunchBrowserAsync(playwright, target);
            var page = await browser.NewPageAsync();
            return new PlaywrightDemoBrowserRuntime(playwright, browser, page);
        }
        catch (PlaywrightException ex) when (NeedsBrowserInstall(ex))
        {
            await EnsureBrowsersInstalledAsync(target);
            var browser = await LaunchBrowserAsync(playwright, target);
            var page = await browser.NewPageAsync();
            return new PlaywrightDemoBrowserRuntime(playwright, browser, page);
        }
    }

    public async Task LoadPageAsync(string pageAssetName, string baseDirectory)
    {
        var pagePath = Path.Combine(baseDirectory, pageAssetName);
        var html = await File.ReadAllTextAsync(pagePath);

        await _page.SetViewportSizeAsync(1280, 840);
        await _page.SetContentAsync(html);
        await _cursor.ResetAsync(_page);
    }

    public async ValueTask DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    private static async Task<IBrowser> LaunchBrowserAsync(IPlaywright playwright, DemoBrowserTarget target)
        => target switch
        {
            DemoBrowserTarget.PlaywrightChromium => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                Args = ["--force-device-scale-factor=1", "--window-size=1480,980"]
            }),
            DemoBrowserTarget.PlaywrightFirefox => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unsupported Playwright demo browser target.")
        };

    private static bool NeedsBrowserInstall(PlaywrightException exception)
        => exception.Message.Contains("Executable doesn't exist", StringComparison.OrdinalIgnoreCase);

    private static async Task EnsureBrowsersInstalledAsync(DemoBrowserTarget target)
    {
        var browserName = target switch
        {
            DemoBrowserTarget.PlaywrightChromium => "chromium",
            DemoBrowserTarget.PlaywrightFirefox => "firefox",
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unsupported Playwright demo browser target.")
        };

        var exitCode = Microsoft.Playwright.Program.Main(["install", browserName]);
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright browser installation failed for '{browserName}' with exit code {exitCode}.");
        }

        await Task.CompletedTask;
    }

    private sealed class PlaywrightDemoCursor : IDemoCursor
    {
        private PlaywrightGhostCursor _cursor;

        public PlaywrightDemoCursor(PlaywrightGhostCursor cursor)
        {
            _cursor = cursor;
        }

        public DefaultOptions? DefaultOptions
        {
            get => _cursor.DefaultOptions;
            set => _cursor.DefaultOptions = value;
        }

        public Task MoveAsync(string selector, MoveOptions? options = null)
            => _cursor.MoveAsync(selector, options);

        public Task ClickAsync(string selector, ClickOptions? options = null)
            => _cursor.ClickAsync(selector, options);

        public Task MouseDownAsync(ClickOptions? options = null)
            => _cursor.MouseDownAsync(options);

        public Task MouseUpAsync(ClickOptions? options = null)
            => _cursor.MouseUpAsync(options);

        public Task ScrollAsync(Vector delta, ScrollOptions? options = null)
            => _cursor.ScrollAsync(delta, options);

        public Task ScrollToAsync(ScrollToDestination destination, ScrollOptions? options = null)
            => _cursor.ScrollToAsync(destination, options);

        public void ToggleRandomMove(bool random)
            => _cursor.ToggleRandomMove(random);

        public async Task ResetAsync(IPage page)
        {
            _cursor = new PlaywrightGhostCursor(page, new Vector(140, 140));
            await _cursor.InstallMouseHelperAsync();
            await page.BringToFrontAsync();
        }
    }
}
