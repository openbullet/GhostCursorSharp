using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace GhostCursorSharp.Tests;

internal sealed class SeleniumBrowserTestSession : IBrowserTestSession
{
    private static readonly string FixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "custom-page.html");

    private readonly IWebDriver _driver;

    private SeleniumBrowserTestSession(IWebDriver driver)
    {
        _driver = driver;
    }

    public static Task<SeleniumBrowserTestSession> CreateAsync(BrowserTestCase browserTestCase)
        => Task.FromResult(new SeleniumBrowserTestSession(CreateDriver(browserTestCase)));

    public async Task LoadFixtureAsync()
    {
        var html = await File.ReadAllTextAsync(FixturePath);
        await LoadContentAsync(html);
    }

    public Task LoadContentAsync(string html)
    {
        _driver.SwitchTo().DefaultContent();
        _driver.Manage().Window.Size = new System.Drawing.Size(800, 600);
        return Task.Run(() => _driver.Navigate().GoToUrl(ToDataUrl(html)));
    }

    public Task<object?> QuerySelectorAsync(string selector)
    {
        _driver.SwitchTo().DefaultContent();
        return Task.FromResult(_driver.FindElements(By.CssSelector(selector)).FirstOrDefault() as object);
    }

    public Task<object> QuerySelectorInFrameAsync(string frameSelector, string selector)
    {
        _driver.SwitchTo().DefaultContent();
        var frameElement = _driver.FindElement(By.CssSelector(frameSelector));
        _driver.SwitchTo().Frame(frameElement);
        return Task.FromResult<object>(_driver.FindElement(By.CssSelector(selector)));
    }

    public Task<bool> IsIntersectingViewportAsync(object element, double threshold)
    {
        _driver.SwitchTo().DefaultContent();
        var result = ((IJavaScriptExecutor)_driver).ExecuteScript(
            """
            const element = arguments[0];
            const threshold = arguments[1];
            const rect = element.getBoundingClientRect();
            return rect.bottom > threshold &&
              rect.right > threshold &&
              rect.top < window.innerHeight - threshold &&
              rect.left < window.innerWidth - threshold;
            """,
            element,
            threshold);
        return Task.FromResult(ConvertResult<bool>(result));
    }

    public Task<T> EvaluateExpressionAsync<T>(string script)
    {
        _driver.SwitchTo().DefaultContent();
        return Task.FromResult(ConvertResult<T>(((IJavaScriptExecutor)_driver).ExecuteScript($"return ({script});")));
    }

    public Task<T> EvaluateFunctionAsync<T>(string script, object? arg = null)
    {
        _driver.SwitchTo().DefaultContent();
        var javascriptExecutor = (IJavaScriptExecutor)_driver;
        var result = javascriptExecutor.ExecuteScript(
            """
            const serializedArg = arguments[0];
            const parsedArg = serializedArg === null || serializedArg === undefined
              ? null
              : JSON.parse(serializedArg);
            const userFunction = SCRIPT_PLACEHOLDER;
            return userFunction(parsedArg);
            """.Replace("SCRIPT_PLACEHOLDER", script),
            arg is null ? null : System.Text.Json.JsonSerializer.Serialize(arg));
        return Task.FromResult(ConvertResult<T>(result));
    }

    public ITestCursor CreateCursor(GhostCursorOptions? options = null)
        => new SeleniumTestCursor(options is null ? new SeleniumGhostCursor(_driver) : new SeleniumGhostCursor(_driver, options));

    public ValueTask DisposeAsync()
    {
        _driver.Quit();
        _driver.Dispose();
        return ValueTask.CompletedTask;
    }

    private static IWebDriver CreateDriver(BrowserTestCase browserTestCase)
        => browserTestCase switch
        {
            BrowserTestCase.SeleniumChromium => CreateChromiumDriver(),
            BrowserTestCase.SeleniumFirefox => CreateFirefoxDriver(),
            _ => throw new ArgumentOutOfRangeException(nameof(browserTestCase), browserTestCase, "Unsupported Selenium browser test case.")
        };

    private static IWebDriver CreateChromiumDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1280,840");
        options.AddArgument("--disable-search-engine-choice-screen");

        if (OperatingSystem.IsLinux())
        {
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
        }

        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;
        return new ChromeDriver(service, options);
    }

    private static IWebDriver CreateFirefoxDriver()
    {
        var options = new FirefoxOptions();
        options.AddArgument("--headless");

        var service = FirefoxDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;
        return new FirefoxDriver(service, options);
    }

    private static string ToDataUrl(string html)
        => "data:text/html;charset=utf-8;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(html));

    private static T ConvertResult<T>(object? result)
        => SeleniumScriptResultConverter.Convert<T>(result);

    private sealed class SeleniumTestCursor : ITestCursor
    {
        private readonly SeleniumGhostCursor _cursor;

        public SeleniumTestCursor(SeleniumGhostCursor cursor)
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
            => await _cursor.GetElementAsync((IWebElement)element, options);

        public async Task<CursorBox> GetElementBoxAsync(object element, bool relativeToMainFrame = true)
        {
            var box = await _cursor.GetElementBoxAsync((IWebElement)element, relativeToMainFrame);
            return new CursorBox(box.X, box.Y, box.Width, box.Height);
        }

        public Task MoveAsync(string selector, MoveOptions? options = null)
            => _cursor.MoveAsync(selector, options);

        public Task MoveAsync(object element, MoveOptions? options = null)
            => _cursor.MoveAsync((IWebElement)element, options);

        public Task ClickAsync(string selector, ClickOptions? options = null)
            => _cursor.ClickAsync(selector, options);

        public Task ClickAsync(object element, ClickOptions? options = null)
            => _cursor.ClickAsync((IWebElement)element, options);

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
