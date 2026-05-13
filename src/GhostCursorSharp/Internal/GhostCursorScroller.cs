using PuppeteerSharp;

namespace GhostCursorSharp.Internal;

internal sealed class GhostCursorScroller
{
    private readonly GhostCursorElementGeometry _geometry;
    private readonly GhostCursorState _state;

    public GhostCursorScroller(GhostCursorState state, GhostCursorElementGeometry geometry)
    {
        _state = state;
        _geometry = geometry;
    }

    public async Task ScrollIntoViewAsync(IElementHandle element, ResolvedScrollIntoViewOptions options)
    {
        var viewport = await GetViewportMetricsAsync();
        var elementBox = await _geometry.GetElementBoxAsync(element);
        var box = ToBoxEdges(elementBox);

        var targetBox = new BoxEdges(
            Top: Math.Max(box.Top - options.InViewportMargin + viewport.ScrollPositionTop, 0) - viewport.ScrollPositionTop,
            Left: Math.Max(box.Left - options.InViewportMargin + viewport.ScrollPositionLeft, 0) - viewport.ScrollPositionLeft,
            Bottom: Math.Min(box.Bottom + options.InViewportMargin + viewport.ScrollPositionTop, viewport.DocumentHeight) - viewport.ScrollPositionTop,
            Right: Math.Min(box.Right + options.InViewportMargin + viewport.ScrollPositionLeft, viewport.DocumentWidth) - viewport.ScrollPositionLeft);

        var isInViewport = targetBox.Top >= 0 &&
                           targetBox.Left >= 0 &&
                           targetBox.Bottom <= viewport.ViewportHeight &&
                           targetBox.Right <= viewport.ViewportWidth;

        if (isInViewport)
        {
            return;
        }

        if (options.InViewportMargin <= 0 && options.ScrollSpeed >= 100)
        {
            await element.EvaluateFunctionAsync(
                "(e) => e.scrollIntoView({ block: 'center', inline: 'center' })");
            await GhostCursorTiming.DelayAsync(options.ScrollDelay);
            return;
        }

        try
        {
            var deltaY = 0d;
            var deltaX = 0d;

            if (targetBox.Top < 0)
            {
                deltaY = targetBox.Top;
            }
            else if (targetBox.Bottom > viewport.ViewportHeight)
            {
                deltaY = targetBox.Bottom - viewport.ViewportHeight;
            }

            if (targetBox.Left < 0)
            {
                deltaX = targetBox.Left;
            }
            else if (targetBox.Right > viewport.ViewportWidth)
            {
                deltaX = targetBox.Right - viewport.ViewportWidth;
            }

            await ScrollAsync(new Vector(deltaX, deltaY), options);
        }
        catch
        {
            await element.EvaluateFunctionAsync(
                "(e, smooth) => e.scrollIntoView({ block: 'center', inline: 'center', behavior: smooth ? 'smooth' : 'auto' })",
                options.ScrollSpeed < 90);
            await GhostCursorTiming.DelayAsync(options.ScrollDelay);
        }
    }

    public async Task ScrollAsync(Vector delta, ResolvedScrollOptions options)
    {
        var absoluteX = Math.Abs(delta.X);
        var absoluteY = Math.Abs(delta.Y);
        var viewport = await GetViewportMetricsAsync();
        var startScrollX = viewport.ScrollPositionLeft;
        var startScrollY = viewport.ScrollPositionTop;
        var anchorX = Math.Clamp(_state.Location.X, 1, Math.Max(1, viewport.ViewportWidth - 1));
        var anchorY = Math.Clamp(_state.Location.Y, 1, Math.Max(1, viewport.ViewportHeight - 1));

        if (Math.Abs(_state.Location.X - anchorX) > double.Epsilon ||
            Math.Abs(_state.Location.Y - anchorY) > double.Epsilon)
        {
            await _state.Page.Mouse.MoveAsync(
                Convert.ToDecimal(anchorX),
                Convert.ToDecimal(anchorY),
                new PuppeteerSharp.Input.MoveOptions { Steps = 1 });

            _state.Location = new Vector(anchorX, anchorY);
        }

        if (absoluteX < double.Epsilon && absoluteY < double.Epsilon)
        {
            await GhostCursorTiming.DelayAsync(options.ScrollDelay);
            return;
        }

        var xDirection = delta.X < 0 ? -1 : 1;
        var yDirection = delta.Y < 0 ? -1 : 1;
        var largerIsX = absoluteX > absoluteY;
        var largerDistance = largerIsX ? absoluteX : absoluteY;
        var shorterDistance = largerIsX ? absoluteY : absoluteX;
        var largerStep = options.ScrollSpeed < 90
            ? options.ScrollSpeed
            : Scale(options.ScrollSpeed, 90, 100, 90, Math.Max(largerDistance, 90));
        var steps = Math.Max(1, (int)Math.Ceiling(largerDistance / Math.Max(largerStep, 1)));
        var previousLong = 0d;
        var previousShort = 0d;

        for (var stepIndex = 1; stepIndex <= steps; stepIndex++)
        {
            var nextLong = (largerDistance * stepIndex) / steps;
            var nextShort = (shorterDistance * stepIndex) / steps;
            var longDelta = nextLong - previousLong;
            var shortDelta = nextShort - previousShort;

            double stepX;
            double stepY;

            if (largerIsX)
            {
                stepX = longDelta * xDirection;
                stepY = shortDelta * yDirection;
            }
            else
            {
                stepX = shortDelta * xDirection;
                stepY = longDelta * yDirection;
            }

            await _state.Page.Mouse.WheelAsync(Convert.ToDecimal(stepX), Convert.ToDecimal(stepY));

            previousLong = nextLong;
            previousShort = nextShort;
        }

        var endViewport = await GetViewportMetricsAsync();
        var verticalMoved = Math.Abs(endViewport.ScrollPositionTop - startScrollY) > 0.5;
        var horizontalMoved = Math.Abs(endViewport.ScrollPositionLeft - startScrollX) > 0.5;

        if ((!verticalMoved && absoluteY > 0) || (!horizontalMoved && absoluteX > 0))
        {
            await _state.Page.EvaluateFunctionAsync(
                "(x, y, smooth) => window.scrollBy({ left: x, top: y, behavior: smooth ? 'smooth' : 'auto' })",
                delta.X,
                delta.Y,
                options.ScrollSpeed < 90);
        }

        await GhostCursorTiming.DelayAsync(options.ScrollDelay);
    }

    public async Task<Vector> GetRandomViewportPointAsync()
    {
        var viewport = await GetViewportMetricsAsync();
        return new Vector(
            Random.Shared.NextDouble() * Math.Max(1, viewport.ViewportWidth),
            Random.Shared.NextDouble() * Math.Max(1, viewport.ViewportHeight));
    }

    public async Task ScrollToAsync(string destination, ResolvedScrollOptions options)
    {
        var viewport = await GetViewportMetricsAsync();
        var namedTarget = destination.ToLowerInvariant() switch
        {
            "top" => new ScrollToDestination { Y = 0 },
            "bottom" => new ScrollToDestination { Y = viewport.DocumentHeight },
            "left" => new ScrollToDestination { X = 0 },
            "right" => new ScrollToDestination { X = viewport.DocumentWidth },
            _ => throw new ArgumentException(
                "Named scroll destinations must be one of: top, bottom, left, right.",
                nameof(destination))
        };

        await ScrollToAsync(namedTarget, options);
    }

    public async Task ScrollToAsync(ScrollToDestination destination, ResolvedScrollOptions options)
    {
        var viewport = await GetViewportMetricsAsync();

        await ScrollAsync(
            new Vector(
                (destination.X ?? viewport.ScrollPositionLeft) - viewport.ScrollPositionLeft,
                (destination.Y ?? viewport.ScrollPositionTop) - viewport.ScrollPositionTop),
            options);
    }

    private async Task<ViewportMetrics> GetViewportMetricsAsync()
        => await _state.Page.EvaluateFunctionAsync<ViewportMetrics>(
            """
            () => ({
              viewportWidth: window.innerWidth,
              viewportHeight: window.innerHeight,
              documentHeight: Math.max(document.documentElement.scrollHeight, document.body.scrollHeight),
              documentWidth: Math.max(document.documentElement.scrollWidth, document.body.scrollWidth),
              scrollPositionTop: window.scrollY,
              scrollPositionLeft: window.scrollX
            })
            """);

    private static BoxEdges ToBoxEdges(BoundingBox box)
        => new(
            Top: Convert.ToDouble(box.Y),
            Left: Convert.ToDouble(box.X),
            Bottom: Convert.ToDouble(box.Y + box.Height),
            Right: Convert.ToDouble(box.X + box.Width));

    private static double Scale(double value, double fromStart, double fromEnd, double toStart, double toEnd)
        => toStart + (((value - fromStart) / (fromEnd - fromStart)) * (toEnd - toStart));

    private sealed record ViewportMetrics(
        double ViewportWidth,
        double ViewportHeight,
        double DocumentHeight,
        double DocumentWidth,
        double ScrollPositionTop,
        double ScrollPositionLeft);

    private readonly record struct BoxEdges(double Top, double Left, double Bottom, double Right);
}
