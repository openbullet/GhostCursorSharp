using PuppeteerSharp;

namespace GhostCursorSharp.Tests;

public class GhostCursorIntegrationTests
{
    private static readonly string FixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "custom-page.html");
    private static readonly string BrowserCachePath = Path.Combine(AppContext.BaseDirectory, ".browser");

    [Fact]
    public async Task GetElementAsync_WaitsForDelayedSelectorFromDefaults()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadFixtureAsync(page);

        await page.EvaluateFunctionAsync(
            """
            () => {
              setTimeout(() => {
                const button = document.createElement('button');
                button.id = 'delayed-button';
                button.textContent = 'Delayed';
                document.body.appendChild(button);
              }, 150);
            }
            """);

        var cursor = new GhostCursor(page, new GhostCursorOptions
        {
            DefaultOptions = new DefaultOptions
            {
                GetElement = new GetElementOptions
                {
                    WaitForSelector = 1000
                }
            }
        });

        var button = await cursor.GetElementAsync("#delayed-button");

        Assert.NotNull(button);
    }

    [Fact]
    public async Task ClickAsync_ClicksElementWithCssSelector()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadFixtureAsync(page);

        var cursor = new GhostCursor(page);
        await cursor.ClickAsync("#box1", FastClickOptions());

        Assert.True(await page.EvaluateExpressionAsync<bool>("window.boxWasClicked"));
    }

    [Fact]
    public async Task ClickAsync_ClicksElementWithXPathSelector()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadFixtureAsync(page);

        var cursor = new GhostCursor(page);
        await cursor.ClickAsync("//*[@id='box1']", FastClickOptions());

        Assert.True(await page.EvaluateExpressionAsync<bool>("window.boxWasClicked"));
    }

    [Fact]
    public async Task MoveAsync_ScrollsOffscreenElementsIntoView()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadFixtureAsync(page);

        var cursor = new GhostCursor(page, new GhostCursorOptions
        {
            DefaultOptions = new DefaultOptions
            {
                Move = new MoveOptions
                {
                    ScrollDelay = 0,
                    ScrollSpeed = 99,
                    InViewportMargin = 50,
                    MoveSpeed = 99,
                    MoveDelay = 0,
                    RandomizeMoveDelay = false,
                    DelayPerStep = 0,
                    PaddingPercentage = 100
                }
            }
        });
        var box2 = await page.QuerySelectorAsync("#box2") ?? throw new InvalidOperationException("box2 not found");
        var box3 = await page.QuerySelectorAsync("#box3") ?? throw new InvalidOperationException("box3 not found");

        Assert.False(await box2.IsIntersectingViewportAsync(0));
        await cursor.MoveAsync(box2);
        Assert.Equal(2500, await page.EvaluateExpressionAsync<int>("Math.round(window.scrollY)"));
        Assert.Equal(0, await page.EvaluateExpressionAsync<int>("Math.round(window.scrollX)"));
        Assert.True(await box2.IsIntersectingViewportAsync(0));

        Assert.False(await box3.IsIntersectingViewportAsync(0));
        await cursor.MoveAsync(box3);
        Assert.Equal(4450, await page.EvaluateExpressionAsync<int>("Math.round(window.scrollY)"));
        Assert.Equal(2250, await page.EvaluateExpressionAsync<int>("Math.round(window.scrollX)"));
        Assert.True(await box3.IsIntersectingViewportAsync(0));
    }

    [Fact]
    public async Task ScrollApis_MoveTheViewport()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadFixtureAsync(page);

        var cursor = new GhostCursor(page);

        await cursor.ScrollAsync(new Vector(0, 600), new ScrollOptions
        {
            ScrollDelay = 0,
            ScrollSpeed = 100
        });

        Assert.True(await page.EvaluateExpressionAsync<double>("window.scrollY") > 0);

        await cursor.ScrollToAsync("right", new ScrollOptions
        {
            ScrollDelay = 0,
            ScrollSpeed = 100
        });

        Assert.True(await page.EvaluateExpressionAsync<double>("window.scrollX") > 0);

        await cursor.ScrollToAsync(new ScrollToDestination
        {
            X = 0,
            Y = 0
        }, new ScrollOptions
        {
            ScrollDelay = 0,
            ScrollSpeed = 100
        });

        Assert.Equal(0, await page.EvaluateExpressionAsync<int>("Math.round(window.scrollX)"));
        Assert.Equal(0, await page.EvaluateExpressionAsync<int>("Math.round(window.scrollY)"));
    }

    [Fact]
    public async Task CreateCursor_UsesDefaultClickOptions()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadFixtureAsync(page);

        await page.EvaluateFunctionAsync(
            """
            () => {
              setTimeout(() => {
                const delayed = document.createElement('div');
                delayed.id = 'delayed-box';
                delayed.textContent = 'Delayed Box';
                delayed.style.position = 'absolute';
                delayed.style.left = '320px';
                delayed.style.top = '160px';
                delayed.style.width = '80px';
                delayed.style.height = '40px';
                delayed.style.background = '#ddd';
                delayed.addEventListener('click', () => {
                  window.delayedBoxWasClicked = true;
                });
                document.body.appendChild(delayed);
              }, 150);
              window.delayedBoxWasClicked = false;
            }
            """);

        var cursor = GhostCursor.CreateCursor(
            page,
            defaultOptions: new DefaultOptions
            {
                Click = new ClickOptions
                {
                    WaitForSelector = 1000,
                    MoveSpeed = 99,
                    MoveDelay = 0,
                    RandomizeMoveDelay = false,
                    DelayPerStep = 0,
                    PaddingPercentage = 100,
                    Hesitate = 0,
                    WaitForClick = 0
                }
            });

        await cursor.ClickAsync("#delayed-box");

        Assert.True(await page.EvaluateExpressionAsync<bool>("window.delayedBoxWasClicked"));
    }

    [Fact]
    public async Task PerformRandomMoves_StartsAndCanBeStopped()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadFixtureAsync(page);

        var cursor = new GhostCursor(page, new GhostCursorOptions
        {
            Start = new Vector(10, 10),
            PerformRandomMoves = true,
            DefaultOptions = new DefaultOptions
            {
                RandomMove = new RandomMoveOptions
                {
                    MoveSpeed = 99,
                    MoveDelay = 25,
                    RandomizeMoveDelay = false,
                    DelayPerStep = 0
                }
            }
        });

        var moved = await WaitUntilAsync(
            () => cursor.GetLocation() != new Vector(10, 10),
            TimeSpan.FromSeconds(2));

        Assert.True(moved);

        cursor.ToggleRandomMove(false);
        var stopped = await WaitForStableLocationAsync(
            cursor,
            TimeSpan.FromMilliseconds(150),
            TimeSpan.FromSeconds(2));

        Assert.True(stopped);
    }

    private static ClickOptions FastClickOptions()
        => new()
        {
            MoveSpeed = 99,
            MoveDelay = 0,
            RandomizeMoveDelay = false,
            DelayPerStep = 0,
            PaddingPercentage = 100,
            WaitForSelector = 500,
            Hesitate = 0,
            WaitForClick = 0
        };

    private static async Task LoadFixtureAsync(IPage page)
    {
        var html = await File.ReadAllTextAsync(FixturePath);
        await page.SetViewportAsync(new ViewPortOptions
        {
            Width = 800,
            Height = 600
        });
        await page.GoToAsync("data:text/html," + Uri.EscapeDataString(html));
    }

    private static async Task<IBrowser> LaunchBrowserAsync()
    {
        var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
        {
            Path = BrowserCachePath
        });

        var installedBrowser = browserFetcher.GetInstalledBrowsers().FirstOrDefault()
            ?? await browserFetcher.DownloadAsync();

        return await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = installedBrowser.GetExecutablePath(),
            DefaultViewport = null
        });
    }

    private static async Task<bool> WaitUntilAsync(Func<bool> predicate, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            if (predicate())
            {
                return true;
            }

            await Task.Delay(25);
        }

        return predicate();
    }

    private static async Task<bool> WaitForStableLocationAsync(
        GhostCursor cursor,
        TimeSpan stableDuration,
        TimeSpan timeout)
    {
        var lastLocation = cursor.GetLocation();
        var stableSince = DateTime.UtcNow;
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            await Task.Delay(25, TestContext.Current.CancellationToken);

            var currentLocation = cursor.GetLocation();
            if (currentLocation != lastLocation)
            {
                lastLocation = currentLocation;
                stableSince = DateTime.UtcNow;
                continue;
            }

            if (DateTime.UtcNow - stableSince >= stableDuration)
            {
                return true;
            }
        }

        return false;
    }
}
