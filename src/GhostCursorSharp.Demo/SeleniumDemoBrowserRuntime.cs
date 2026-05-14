using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace GhostCursorSharp.Demo;

internal sealed class SeleniumDemoBrowserRuntime : IDemoBrowserRuntime
{
    private readonly IWebDriver _driver;
    private readonly SeleniumDemoCursor _cursor;

    private SeleniumDemoBrowserRuntime(IWebDriver driver)
    {
        _driver = driver;
        _cursor = new SeleniumDemoCursor(new SeleniumGhostCursor(driver, new Vector(140, 140)));
    }

    public IDemoCursor Cursor => _cursor;

    public static Task<SeleniumDemoBrowserRuntime> CreateAsync(DemoBrowserTarget target)
        => Task.FromResult(new SeleniumDemoBrowserRuntime(CreateDriver(target)));

    public async Task LoadPageAsync(string pageAssetName, string baseDirectory)
    {
        var pagePath = Path.Combine(baseDirectory, pageAssetName);
        var html = await File.ReadAllTextAsync(pagePath);

        _driver.SwitchTo().DefaultContent();
        _driver.Manage().Window.Size = new System.Drawing.Size(1280, 840);
        await Task.Run(() => _driver.Navigate().GoToUrl(ToDataUrl(html)));
        await _cursor.ResetAsync(_driver);
    }

    public ValueTask DisposeAsync()
    {
        _driver.Quit();
        _driver.Dispose();
        return ValueTask.CompletedTask;
    }

    private static IWebDriver CreateDriver(DemoBrowserTarget target)
        => target switch
        {
            DemoBrowserTarget.SeleniumChromium => CreateChromiumDriver(),
            DemoBrowserTarget.SeleniumFirefox => CreateFirefoxDriver(),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unsupported Selenium demo browser target.")
        };

    private static IWebDriver CreateChromiumDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--force-device-scale-factor=1");
        options.AddArgument("--window-size=1480,980");
        options.AddArgument("--disable-search-engine-choice-screen");

        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;
        return new ChromeDriver(service, options);
    }

    private static IWebDriver CreateFirefoxDriver()
    {
        var options = new FirefoxOptions();
        var service = FirefoxDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;

        return new FirefoxDriver(service, options);
    }

    private static string ToDataUrl(string html)
        => "data:text/html;charset=utf-8;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html));

    private sealed class SeleniumDemoCursor : IDemoCursor
    {
        private SeleniumGhostCursor _cursor;

        public SeleniumDemoCursor(SeleniumGhostCursor cursor)
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

        public async Task ResetAsync(IWebDriver driver)
        {
            _cursor = new SeleniumGhostCursor(driver, new Vector(140, 140));
            await _cursor.InstallMouseHelperAsync();
        }
    }
}
