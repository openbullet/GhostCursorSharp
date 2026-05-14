using PuppeteerSharp;

namespace GhostCursorSharp.Demo;

internal sealed class PuppeteerDemoBrowserRuntime : IDemoBrowserRuntime
{
    private readonly IBrowser _browser;
    private readonly IPage _page;
    private readonly PuppeteerDemoCursor _cursor;

    private PuppeteerDemoBrowserRuntime(IBrowser browser, IPage page)
    {
        _browser = browser;
        _page = page;
        _cursor = new PuppeteerDemoCursor(new GhostCursor(page, new Vector(140, 140)));
    }

    public IDemoCursor Cursor => _cursor;

    public static async Task<PuppeteerDemoBrowserRuntime> CreateAsync()
    {
        var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
        {
            Path = Path.Combine(AppContext.BaseDirectory, ".browser")
        });

        var installedBrowser = browserFetcher.GetInstalledBrowsers().FirstOrDefault()
            ?? await browserFetcher.DownloadAsync();

        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = false,
            ExecutablePath = installedBrowser.GetExecutablePath(),
            DefaultViewport = null,
            Args =
            [
                "--force-device-scale-factor=1",
                "--window-size=1480,980"
            ]
        });

        var page = (await browser.PagesAsync()).FirstOrDefault() ?? await browser.NewPageAsync();
        return new PuppeteerDemoBrowserRuntime(browser, page);
    }

    public async Task LoadPageAsync(string pageAssetName, string baseDirectory)
    {
        var pagePath = Path.Combine(baseDirectory, pageAssetName);
        var html = await File.ReadAllTextAsync(pagePath);

        await _page.SetViewportAsync(new ViewPortOptions
        {
            Width = 1280,
            Height = 840
        });

        await _page.SetContentAsync(html);
        await _cursor.ResetAsync(_page);
    }

    public async ValueTask DisposeAsync()
        => await _browser.CloseAsync();

    private sealed class PuppeteerDemoCursor : IDemoCursor
    {
        private GhostCursor _cursor;

        public PuppeteerDemoCursor(GhostCursor cursor)
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
            _cursor = new GhostCursor(page, new Vector(140, 140));
            await _cursor.InstallMouseHelperAsync();
            await page.BringToFrontAsync();
        }
    }
}
