namespace GhostCursorSharp.Tests;

public class GhostCursorIntegrationTests
{
    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task GetElementAsync_WaitsForDelayedSelectorFromDefaults(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadFixtureAsync();

        await session.EvaluateFunctionAsync<object>(
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

        var cursor = session.CreateCursor(new GhostCursorOptions
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

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task GetElementAsync_WithElementHandle_ReturnsTheSameHandle(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadFixtureAsync();

        var cursor = session.CreateCursor();
        var box = await session.QuerySelectorAsync("#box1")
            ?? throw new InvalidOperationException("box1 not found");

        var resolved = await cursor.GetElementAsync(box, new GetElementOptions
        {
            WaitForSelector = 1
        });

        Assert.Same(box, resolved);
    }

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task ClickAsync_ClicksElementWithCssSelector(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadFixtureAsync();

        var cursor = session.CreateCursor();
        await cursor.ClickAsync("#box1", FastClickOptions());

        Assert.True(await session.EvaluateExpressionAsync<bool>("window.boxWasClicked"));
    }

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task ClickAsync_ClicksElementWithXPathSelector(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadFixtureAsync();

        var cursor = session.CreateCursor();
        await cursor.ClickAsync("//*[@id='box1']", FastClickOptions());

        Assert.True(await session.EvaluateExpressionAsync<bool>("window.boxWasClicked"));
    }

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task MoveAsync_TargetsAVisibleInlineFragment(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadContentAsync(
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

        var cursor = session.CreateCursor(new GhostCursorOptions
        {
            Start = new Vector(10, 10),
            DefaultOptions = new DefaultOptions
            {
                Move = FastMoveOptions()
            }
        });

        var inlineLink = await session.QuerySelectorAsync("#inline-link")
            ?? throw new InvalidOperationException("inline-link not found");

        await cursor.MoveAsync(inlineLink);

        var intersectsClientRect = await session.EvaluateFunctionAsync<bool>(
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

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task GetElementBoxAsync_UsesInlineElementGeometry(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadContentAsync(
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

        var cursor = session.CreateCursor();
        var inlineLink = await session.QuerySelectorAsync("#inline-link")
            ?? throw new InvalidOperationException("inline-link not found");

        var box = await cursor.GetElementBoxAsync(inlineLink);
        var centerX = box.X + (box.Width / 2);
        var centerY = box.Y + (box.Height / 2);

        var centerIntersectsClientRect = await session.EvaluateFunctionAsync<bool>(
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

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task MoveAsync_ScrollsOffscreenElementsIntoView(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadFixtureAsync();

        var cursor = session.CreateCursor(new GhostCursorOptions
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
        var box2 = await session.QuerySelectorAsync("#box2") ?? throw new InvalidOperationException("box2 not found");
        var box3 = await session.QuerySelectorAsync("#box3") ?? throw new InvalidOperationException("box3 not found");

        Assert.False(await IsSelectorInViewportAsync(session, "#box2"));
        await cursor.MoveAsync(box2);
        var scrollYAfterBox2 = await session.EvaluateExpressionAsync<int>("Math.round(window.scrollY)");
        var scrollXAfterBox2 = await session.EvaluateExpressionAsync<int>("Math.round(window.scrollX)");
        Assert.True(scrollYAfterBox2 > 0);
        Assert.Equal(0, scrollXAfterBox2);

        await cursor.MoveAsync(box3);
        var scrollYAfterBox3 = await session.EvaluateExpressionAsync<int>("Math.round(window.scrollY)");
        var scrollXAfterBox3 = await session.EvaluateExpressionAsync<int>("Math.round(window.scrollX)");
        Assert.True(scrollYAfterBox3 >= scrollYAfterBox2);
        Assert.True(scrollXAfterBox3 > scrollXAfterBox2);
    }

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task ClickAsync_ClicksElementInsideIframe(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadContentAsync(
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

        var frameButton = await session.QuerySelectorInFrameAsync("#demo-frame", "#frame-button");

        var cursor = session.CreateCursor(new GhostCursorOptions
        {
            DefaultOptions = new DefaultOptions
            {
                Click = FastClickOptions()
            }
        });

        await cursor.ClickAsync(frameButton);

        Assert.True(await session.EvaluateExpressionAsync<bool>("window.iframeClicked"));
    }

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task MoveAsync_RetriesWhenTheElementMovesDuringThePath(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadContentAsync(
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

        var cursor = session.CreateCursor(new GhostCursorOptions
        {
            Start = new Vector(5, 5),
            DefaultOptions = new DefaultOptions
            {
                Move = RetryMoveOptions()
            }
        });

        var movingBox = await session.QuerySelectorAsync("#moving-box")
            ?? throw new InvalidOperationException("moving-box not found");

        await cursor.MoveAsync(movingBox);

        var endedInsideMovedElement = await session.EvaluateFunctionAsync<bool>(
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

        Assert.True(await session.EvaluateExpressionAsync<bool>("window.moveEventCount === 1"));
        Assert.True(endedInsideMovedElement);
    }

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task MoveAsync_WithSelector_ReacquiresElementWhenItIsReplaced(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadContentAsync(
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

        var cursor = session.CreateCursor(new GhostCursorOptions
        {
            Start = new Vector(5, 5),
            DefaultOptions = new DefaultOptions
            {
                Move = RetryMoveOptions()
            }
        });

        await cursor.MoveAsync("#moving-box");

        var endedInsideReplacement = await session.EvaluateFunctionAsync<bool>(
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

        Assert.True(await session.EvaluateExpressionAsync<bool>("window.replacementCount === 1"));
        Assert.True(endedInsideReplacement);
    }

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task ClickAsync_WithSelector_ReacquiresElementWhenItIsReplaced(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadContentAsync(
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

        var cursor = session.CreateCursor(new GhostCursorOptions
        {
            Start = new Vector(5, 5),
            DefaultOptions = new DefaultOptions
            {
                Click = RetryClickOptions()
            }
        });

        await cursor.ClickAsync("#replaceable-button");

        Assert.True(await session.EvaluateExpressionAsync<bool>("window.buttonReplacementCount === 1"));
        Assert.True(await session.EvaluateExpressionAsync<bool>("window.replacementClicked"));
    }

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task ScrollApis_MoveTheViewport(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadFixtureAsync();

        var cursor = session.CreateCursor();

        await cursor.ScrollAsync(new Vector(0, 600), new ScrollOptions
        {
            ScrollDelay = 0,
            ScrollSpeed = 100
        });

        Assert.True(await session.EvaluateExpressionAsync<double>("window.scrollY") > 0);

        await cursor.ScrollToAsync("right", new ScrollOptions
        {
            ScrollDelay = 0,
            ScrollSpeed = 100
        });

        Assert.True(await session.EvaluateExpressionAsync<double>("window.scrollX") > 0);

        await cursor.ScrollToAsync(new ScrollToDestination
        {
            X = 0,
            Y = 0
        }, new ScrollOptions
        {
            ScrollDelay = 0,
            ScrollSpeed = 100
        });

        Assert.Equal(0, await session.EvaluateExpressionAsync<int>("Math.round(window.scrollX)"));
        Assert.Equal(0, await session.EvaluateExpressionAsync<int>("Math.round(window.scrollY)"));
    }

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task CreateCursor_UsesDefaultClickOptions(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadFixtureAsync();

        await session.EvaluateFunctionAsync<object>(
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

        var cursor = session.CreateCursor(new GhostCursorOptions
        {
            DefaultOptions = new DefaultOptions
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
            }
        });

        await cursor.ClickAsync("#delayed-box");

        Assert.True(await session.EvaluateExpressionAsync<bool>("window.delayedBoxWasClicked"));
    }

    [Theory]
    [MemberData(nameof(BrowserTestCases.All), MemberType = typeof(BrowserTestCases))]
    public async Task PerformRandomMoves_StartsAndCanBeStopped(BrowserTestCase browserTestCase)
    {
        await using var session = await BrowserTestSessionFactory.CreateAsync(browserTestCase);
        await session.LoadFixtureAsync();

        var cursor = session.CreateCursor(new GhostCursorOptions
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

    private static Task<bool> IsSelectorInViewportAsync(IBrowserTestSession session, string selector)
        => session.EvaluateFunctionAsync<bool>(
            """
            (targetSelector) => {
              const element = document.querySelector(targetSelector);
              if (!element) {
                return false;
              }

              const rect = element.getBoundingClientRect();
              return rect.bottom > 0 &&
                rect.right > 0 &&
                rect.top < window.innerHeight &&
                rect.left < window.innerWidth;
            }
            """,
            selector);

    private static async Task<bool> WaitForStableLocationAsync(
        ITestCursor cursor,
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
