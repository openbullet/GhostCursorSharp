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
    public async Task GetElementAsync_WithElementHandle_ReturnsTheSameHandle()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadFixtureAsync(page);

        var cursor = new GhostCursor(page);
        var box = await page.QuerySelectorAsync("#box1")
            ?? throw new InvalidOperationException("box1 not found");

        var resolved = await cursor.GetElementAsync(box, new GetElementOptions
        {
            WaitForSelector = 1
        });

        Assert.Same(box, resolved);
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
    public async Task MoveAsync_TargetsAVisibleInlineFragment()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadContentAsync(
            page,
            """
            <html lang="en">
            <body style="margin: 0; font: 16px/1.4 sans-serif;">
              <div style="width: 110px; margin: 48px;">
                <a id="inline-link" href="#" style="color: #06c;">
                  Ghost cursor should target a visible wrapped inline fragment reliably.
                </a>
              </div>
            </body>
            </html>
            """);

        var cursor = new GhostCursor(page, new GhostCursorOptions
        {
            Start = new Vector(10, 10),
            DefaultOptions = new DefaultOptions
            {
                Move = FastMoveOptions()
            }
        });

        var inlineLink = await page.QuerySelectorAsync("#inline-link")
            ?? throw new InvalidOperationException("inline-link not found");

        await cursor.MoveAsync(inlineLink);

        var intersectsClientRect = await page.EvaluateFunctionAsync<bool>(
            """
            (point) => {
              const rects = [...document.querySelector('#inline-link').getClientRects()];
              return rects.some(rect =>
                point.x > rect.left &&
                point.x <= rect.right &&
                point.y > rect.top &&
                point.y <= rect.bottom);
            }
            """,
            new { x = cursor.Location.X, y = cursor.Location.Y });

        Assert.True(intersectsClientRect);
    }

    [Fact]
    public async Task GetElementBoxAsync_UsesInlineElementGeometry()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadContentAsync(
            page,
            """
            <html lang="en">
            <body style="margin: 0; font: 16px/1.4 sans-serif;">
              <div style="width: 110px; margin: 48px;">
                <a id="inline-link" href="#" style="color: #06c;">
                  Ghost cursor should expose inline geometry through the public API as well.
                </a>
              </div>
            </body>
            </html>
            """);

        var cursor = new GhostCursor(page);
        var inlineLink = await page.QuerySelectorAsync("#inline-link")
            ?? throw new InvalidOperationException("inline-link not found");

        var box = await cursor.GetElementBoxAsync(inlineLink);
        var centerX = Convert.ToDouble(box.X + (box.Width / 2));
        var centerY = Convert.ToDouble(box.Y + (box.Height / 2));

        var centerIntersectsClientRect = await page.EvaluateFunctionAsync<bool>(
            """
            (point) => {
              const rects = [...document.querySelector('#inline-link').getClientRects()];
              return rects.some(rect =>
                point.x > rect.left &&
                point.x <= rect.right &&
                point.y > rect.top &&
                point.y <= rect.bottom);
            }
            """,
            new { x = centerX, y = centerY });

        Assert.True(box.Width > 0);
        Assert.True(box.Height > 0);
        Assert.True(centerIntersectsClientRect);
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
    public async Task ClickAsync_ClicksElementInsideIframe()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadContentAsync(
            page,
            """
            <html lang="en">
            <body style="margin: 0;">
              <script>window.iframeClicked = false;</script>
              <iframe
                id="demo-frame"
                style="margin-left: 120px; margin-top: 80px; width: 320px; height: 220px;"
                srcdoc="
                  <!doctype html>
                  <html lang='en'>
                  <body style='margin: 0; position: relative; height: 100vh;'>
                    <button
                      id='frame-button'
                      style='position: absolute; left: 80px; top: 60px; width: 120px; height: 48px;'
                      onclick='window.parent.iframeClicked = true'>
                      Frame button
                    </button>
                  </body>
                  </html>">
              </iframe>
            </body>
            </html>
            """);

        var frameElement = await page.QuerySelectorAsync("#demo-frame")
            ?? throw new InvalidOperationException("demo-frame not found");
        var frame = await frameElement.ContentFrameAsync()
            ?? throw new InvalidOperationException("iframe content frame not available");
        var frameButton = await frame.QuerySelectorAsync("#frame-button")
            ?? throw new InvalidOperationException("frame-button not found");

        var cursor = new GhostCursor(page, new GhostCursorOptions
        {
            DefaultOptions = new DefaultOptions
            {
                Click = FastClickOptions()
            }
        });

        await cursor.ClickAsync(frameButton);

        Assert.True(await page.EvaluateExpressionAsync<bool>("window.iframeClicked"));
    }

    [Fact]
    public async Task MoveAsync_RetriesWhenTheElementMovesDuringThePath()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadContentAsync(
            page,
            """
            <html lang="en">
            <body style="margin: 0; overflow: hidden;">
              <div
                id="moving-box"
                style="position: absolute; left: 520px; top: 80px; width: 80px; height: 80px; background: #eee;">
              </div>
              <script>
                window.moveEventCount = 0;
                setTimeout(() => {
                  const box = document.querySelector('#moving-box');
                  box.style.left = '160px';
                  box.style.top = '280px';
                  window.moveEventCount += 1;
                }, 30);
              </script>
            </body>
            </html>
            """);

        var cursor = new GhostCursor(page, new GhostCursorOptions
        {
            Start = new Vector(5, 5),
            DefaultOptions = new DefaultOptions
            {
                Move = new MoveOptions
                {
                    MoveSpeed = 20,
                    MoveDelay = 0,
                    RandomizeMoveDelay = false,
                    DelayPerStep = 8,
                    ScrollDelay = 0,
                    ScrollSpeed = 100,
                    PaddingPercentage = 100,
                    OvershootThreshold = 10,
                    MaxTries = 2
                }
            }
        });

        var movingBox = await page.QuerySelectorAsync("#moving-box")
            ?? throw new InvalidOperationException("moving-box not found");

        await cursor.MoveAsync(movingBox);

        var endedInsideMovedElement = await page.EvaluateFunctionAsync<bool>(
            """
            (point) => {
              const rect = document.querySelector('#moving-box').getBoundingClientRect();
              return point.x > rect.left &&
                point.x <= rect.right &&
                point.y > rect.top &&
                point.y <= rect.bottom;
            }
            """,
            new { x = cursor.Location.X, y = cursor.Location.Y });

        Assert.True(await page.EvaluateExpressionAsync<bool>("window.moveEventCount === 1"));
        Assert.True(endedInsideMovedElement);
    }

    [Fact]
    public async Task MoveAsync_WithSelector_ReacquiresElementWhenItIsReplaced()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadContentAsync(
            page,
            """
            <html lang="en">
            <body style="margin: 0; overflow: hidden;">
              <div
                id="moving-box"
                style="position: absolute; left: 520px; top: 80px; width: 80px; height: 80px; background: #eee;">
              </div>
              <script>
                window.replacementCount = 0;
                setTimeout(() => {
                  const oldBox = document.querySelector('#moving-box');
                  oldBox.remove();

                  const newBox = document.createElement('div');
                  newBox.id = 'moving-box';
                  newBox.style.position = 'absolute';
                  newBox.style.left = '160px';
                  newBox.style.top = '280px';
                  newBox.style.width = '80px';
                  newBox.style.height = '80px';
                  newBox.style.background = '#ccc';
                  document.body.appendChild(newBox);

                  window.replacementCount += 1;
                }, 30);
              </script>
            </body>
            </html>
            """);

        var cursor = new GhostCursor(page, new GhostCursorOptions
        {
            Start = new Vector(5, 5),
            DefaultOptions = new DefaultOptions
            {
                Move = RetryMoveOptions()
            }
        });

        await cursor.MoveAsync("#moving-box");

        var endedInsideReplacement = await page.EvaluateFunctionAsync<bool>(
            """
            (point) => {
              const rect = document.querySelector('#moving-box').getBoundingClientRect();
              return point.x > rect.left &&
                point.x <= rect.right &&
                point.y > rect.top &&
                point.y <= rect.bottom;
            }
            """,
            new { x = cursor.Location.X, y = cursor.Location.Y });

        Assert.True(await page.EvaluateExpressionAsync<bool>("window.replacementCount === 1"));
        Assert.True(endedInsideReplacement);
    }

    [Fact]
    public async Task ClickAsync_WithSelector_ReacquiresElementWhenItIsReplaced()
    {
        await using var browser = await LaunchBrowserAsync();
        await using var page = await browser.NewPageAsync();
        await LoadContentAsync(
            page,
            """
            <html lang="en">
            <body style="margin: 0; overflow: hidden;">
              <button
                id="replaceable-button"
                style="position: absolute; left: 520px; top: 80px; width: 140px; height: 48px;">
                Old button
              </button>
              <script>
                window.replacementClicked = false;
                window.buttonReplacementCount = 0;
                setTimeout(() => {
                  const oldButton = document.querySelector('#replaceable-button');
                  oldButton.remove();

                  const newButton = document.createElement('button');
                  newButton.id = 'replaceable-button';
                  newButton.textContent = 'New button';
                  newButton.style.position = 'absolute';
                  newButton.style.left = '180px';
                  newButton.style.top = '320px';
                  newButton.style.width = '140px';
                  newButton.style.height = '48px';
                  newButton.addEventListener('click', () => {
                    window.replacementClicked = true;
                  });
                  document.body.appendChild(newButton);

                  window.buttonReplacementCount += 1;
                }, 30);
              </script>
            </body>
            </html>
            """);

        var cursor = new GhostCursor(page, new GhostCursorOptions
        {
            Start = new Vector(5, 5),
            DefaultOptions = new DefaultOptions
            {
                Click = RetryClickOptions()
            }
        });

        await cursor.ClickAsync("#replaceable-button");

        Assert.True(await page.EvaluateExpressionAsync<bool>("window.buttonReplacementCount === 1"));
        Assert.True(await page.EvaluateExpressionAsync<bool>("window.replacementClicked"));
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

    private static MoveOptions FastMoveOptions()
        => new()
        {
            MoveSpeed = 99,
            MoveDelay = 0,
            RandomizeMoveDelay = false,
            DelayPerStep = 0,
            ScrollDelay = 0,
            ScrollSpeed = 100,
            PaddingPercentage = 100
        };

    private static MoveOptions RetryMoveOptions()
        => new()
        {
            MoveSpeed = 20,
            MoveDelay = 0,
            RandomizeMoveDelay = false,
            DelayPerStep = 8,
            ScrollDelay = 0,
            ScrollSpeed = 100,
            PaddingPercentage = 100,
            OvershootThreshold = 10,
            MaxTries = 2
        };

    private static ClickOptions RetryClickOptions()
        => new()
        {
            MoveSpeed = 20,
            MoveDelay = 0,
            RandomizeMoveDelay = false,
            DelayPerStep = 8,
            ScrollDelay = 0,
            ScrollSpeed = 100,
            PaddingPercentage = 100,
            OvershootThreshold = 10,
            MaxTries = 2,
            Hesitate = 0,
            WaitForClick = 0
        };

    private static async Task LoadFixtureAsync(IPage page)
    {
        var html = await File.ReadAllTextAsync(FixturePath);
        await LoadContentAsync(page, html);
    }

    private static async Task LoadContentAsync(IPage page, string html)
    {
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
