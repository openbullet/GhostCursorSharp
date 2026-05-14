using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace GhostCursorSharp.Internal;

internal sealed class SeleniumCursorPageAdapter : ICursorPageAdapter
{
    private readonly IWebDriver _driver;
    private readonly IActionExecutor _actionExecutor;
    private readonly IJavaScriptExecutor _javascriptExecutor;
    private readonly bool _useCurrentContextCoordinates;
    private readonly PointerInputDevice _pointer = new(PointerKind.Mouse, "ghost-cursor");
    private readonly WheelInputDevice _wheel = new("ghost-cursor-wheel");

    public SeleniumCursorPageAdapter(IWebDriver driver)
    {
        _driver = driver;
        _actionExecutor = driver as IActionExecutor
            ?? throw new InvalidOperationException("The Selenium driver must implement IActionExecutor.");
        _javascriptExecutor = driver as IJavaScriptExecutor
            ?? throw new InvalidOperationException("The Selenium driver must implement IJavaScriptExecutor.");
        _useCurrentContextCoordinates = GetBrowserName(driver)
            .Contains("firefox", StringComparison.OrdinalIgnoreCase);
    }

    public string CreateXPathSelector(string selector)
        => $"xpath:{selector}";

    public Task WaitForSelectorAsync(string selector, int timeoutMilliseconds)
    {
        var by = ParseBy(selector);
        var wait = new WebDriverWait(_driver, TimeSpan.FromMilliseconds(timeoutMilliseconds))
        {
            PollingInterval = TimeSpan.FromMilliseconds(50)
        };

        wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
        wait.Until(driver =>
        {
            try
            {
                return driver.FindElements(by).Count > 0;
            }
            catch (WebDriverException)
            {
                return false;
            }
        });

        return Task.CompletedTask;
    }

    public Task<ICursorElementHandle?> QuerySelectorAsync(string selector)
    {
        var element = _driver.FindElements(ParseBy(selector)).FirstOrDefault();
        return Task.FromResult(element is null ? null : (ICursorElementHandle)new SeleniumCursorElementHandle(_driver, element));
    }

    public Task<IReadOnlyList<ICursorElementHandle>> QuerySelectorAllAsync(string selector)
        => Task.FromResult<IReadOnlyList<ICursorElementHandle>>(
            _driver.FindElements(ParseBy(selector))
                .Select(element => (ICursorElementHandle)new SeleniumCursorElementHandle(_driver, element))
                .ToArray());

    public Task MoveMouseAsync(double x, double y, int steps = 1)
    {
        var frameOffset = _useCurrentContextCoordinates
            ? new FrameOffset(0, 0)
            : GetCurrentFrameViewportOffset();
        var viewport = GetCurrentViewportSize(useRootContext: !_useCurrentContextCoordinates);
        var targetX = x + frameOffset.X;
        var targetY = y + frameOffset.Y;

        if (viewport is not null)
        {
            targetX = Math.Clamp(targetX, 0, Math.Max(0, viewport.Width - 1));
            targetY = Math.Clamp(targetY, 0, Math.Max(0, viewport.Height - 1));
        }

        var sequence = new ActionSequence(_pointer);
        sequence.AddAction(_pointer.CreatePointerMove(
            CoordinateOrigin.Viewport,
            (int)Math.Round(targetX),
            (int)Math.Round(targetY),
            TimeSpan.Zero));
        _actionExecutor.PerformActions([sequence]);
        return Task.CompletedTask;
    }

    public Task MouseWheelAsync(double deltaX, double deltaY)
    {
        var wheelDeltaX = (int)Math.Round(deltaX);
        var wheelDeltaY = (int)Math.Round(deltaY);

        if (wheelDeltaX == 0 && wheelDeltaY == 0)
        {
            return Task.CompletedTask;
        }

        var viewport = GetCurrentViewportSize(useRootContext: true);
        var originX = viewport is null ? 0 : (int)Math.Max(0, Math.Round(viewport.Width / 2d));
        var originY = viewport is null ? 0 : (int)Math.Max(0, Math.Round(viewport.Height / 2d));
        var sequence = new ActionSequence(_wheel);
        sequence.AddAction(_wheel.CreateWheelScroll(
            CoordinateOrigin.Viewport,
            originX,
            originY,
            wheelDeltaX,
            wheelDeltaY,
            TimeSpan.FromMilliseconds(24)));
        _actionExecutor.PerformActions([sequence]);
        return Task.CompletedTask;
    }

    public Task MouseDownAsync(MouseButton button, int clickCount)
    {
        var sequence = new ActionSequence(_pointer);
        sequence.AddAction(_pointer.CreatePointerDown(MapButton(button)));
        _actionExecutor.PerformActions([sequence]);
        return Task.CompletedTask;
    }

    public Task MouseUpAsync(MouseButton button, int clickCount)
    {
        var sequence = new ActionSequence(_pointer);
        sequence.AddAction(_pointer.CreatePointerUp(MapButton(button)));
        _actionExecutor.PerformActions([sequence]);
        return Task.CompletedTask;
    }

    public Task<T> EvaluateFunctionAsync<T>(string script, params object?[] args)
        => SeleniumScriptExecutor.EvaluateFunctionAsync<T>(_javascriptExecutor, script, args);

    public Task EvaluateFunctionAsync(string script, params object?[] args)
        => SeleniumScriptExecutor.EvaluateFunctionAsync(_javascriptExecutor, script, args);

    private static By ParseBy(string selector)
        => selector.StartsWith("xpath:", StringComparison.Ordinal)
            ? By.XPath(selector["xpath:".Length..])
            : By.CssSelector(selector);

    private FrameOffset GetCurrentFrameViewportOffset()
    {
        try
        {
            return SeleniumScriptExecutor.ConvertResult<FrameOffset>(_javascriptExecutor.ExecuteScript(
                """
                const offset = { x: 0, y: 0 };
                let currentWindow = window;

                while (currentWindow.frameElement) {
                  const frameRect = currentWindow.frameElement.getBoundingClientRect();
                  offset.x += frameRect.left;
                  offset.y += frameRect.top;

                  try {
                    currentWindow = currentWindow.parent;
                  } catch {
                    break;
                  }
                }

                return offset;
                """));
        }
        catch
        {
            return new FrameOffset(0, 0);
        }
    }

    private ViewportSize? GetCurrentViewportSize(bool useRootContext)
    {
        try
        {
            return SeleniumScriptExecutor.ConvertResult<ViewportSize>(_javascriptExecutor.ExecuteScript(
                useRootContext
                    ? """
                      let currentWindow = window;

                      while (currentWindow.parent && currentWindow.parent !== currentWindow) {
                        try {
                          currentWindow = currentWindow.parent;
                        } catch {
                          break;
                        }
                      }

                      return {
                        width: currentWindow.innerWidth,
                        height: currentWindow.innerHeight
                      };
                      """
                    : """
                      return {
                        width: window.innerWidth,
                        height: window.innerHeight
                      };
                      """));
        }
        catch
        {
            return null;
        }
    }

    private static string GetBrowserName(IWebDriver driver)
        => (driver as IHasCapabilities)?.Capabilities.GetCapability("browserName")?.ToString()
            ?? string.Empty;

    private static OpenQA.Selenium.Interactions.MouseButton MapButton(MouseButton button)
        => button switch
        {
            MouseButton.Left => OpenQA.Selenium.Interactions.MouseButton.Left,
            MouseButton.Middle => OpenQA.Selenium.Interactions.MouseButton.Middle,
            MouseButton.Right => OpenQA.Selenium.Interactions.MouseButton.Right,
            _ => throw new NotSupportedException($"Mouse button '{button}' is not supported by Selenium.")
        };

    private sealed record FrameOffset(double X, double Y);

    private sealed record ViewportSize(double Width, double Height);
}
