using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace GhostCursorSharp.Internal;

internal sealed class GhostCursorMover
{
    private const double OvershootRadius = 120;
    private const double OvershootSpread = 10;

    private readonly GhostCursorElementGeometry _geometry;
    private readonly GhostCursorScroller _scroller;
    private readonly GhostCursorState _state;

    public GhostCursorMover(
        GhostCursorState state,
        GhostCursorScroller scroller,
        GhostCursorElementGeometry geometry)
    {
        _state = state;
        _scroller = scroller;
        _geometry = geometry;
    }

    public async Task MoveToWithAbortAsync(
        Vector destination,
        PathOptions? pathOptions,
        int delayPerStep,
        Func<bool>? shouldAbort)
    {
        var path = CursorPath.Generate(_state.Location, destination, pathOptions);

        foreach (var point in path)
        {
            if (shouldAbort?.Invoke() == true)
            {
                return;
            }

            await _state.Page.Mouse.MoveAsync(
                Convert.ToDecimal(point.X),
                Convert.ToDecimal(point.Y),
                new PuppeteerSharp.Input.MoveOptions { Steps = 1 });

            _state.Location = point;

            if (delayPerStep > 0)
            {
                await Task.Delay(delayPerStep);
            }
        }
    }

    public Task MoveToAsync(Vector destination, PathOptions? pathOptions = null, int delayPerStep = 0)
        => MoveToWithAbortAsync(destination, pathOptions, delayPerStep, null);

    public Task MoveByAsync(Vector delta, PathOptions? pathOptions = null, int delayPerStep = 0)
        => MoveToWithAbortAsync(
            new Vector(_state.Location.X + delta.X, _state.Location.Y + delta.Y),
            pathOptions,
            delayPerStep,
            null);

    public Task MoveByWithAbortAsync(
        Vector delta,
        PathOptions? pathOptions,
        int delayPerStep,
        Func<bool>? shouldAbort)
        => MoveToWithAbortAsync(
            new Vector(_state.Location.X + delta.X, _state.Location.Y + delta.Y),
            pathOptions,
            delayPerStep,
            shouldAbort);

    public async Task MoveAsync(BoundingBox boundingBox, ResolvedMoveOptions options)
    {
        var destination = GetTargetPoint(boundingBox, options);
        await MoveWithOvershootAsync(destination, options);
    }

    public async Task MoveAsync(IElementHandle element, ResolvedMoveOptions options)
        => await MoveAsync(() => Task.FromResult(element), options, canReacquire: false);

    public async Task MoveAsync(Func<Task<IElementHandle>> elementResolver, ResolvedMoveOptions options)
        => await MoveAsync(elementResolver, options, canReacquire: true);

    private async Task MoveAsync(
        Func<Task<IElementHandle>> elementResolver,
        ResolvedMoveOptions options,
        bool canReacquire)
    {
        Exception? lastException = null;

        for (var attempt = 0; attempt <= options.MaxTries; attempt++)
        {
            IElementHandle element;
            try
            {
                element = await elementResolver();
            }
            catch (Exception ex) when (canReacquire && attempt < options.MaxTries)
            {
                lastException = ex;
                continue;
            }

            try
            {
                await _scroller.ScrollIntoViewAsync(element, options);

                var boundingBox = await _geometry.GetElementBoxAsync(element);
                var destination = GetTargetPoint(boundingBox, options);

                await MoveWithOvershootAsync(destination, options);

                var updatedBoundingBox = await _geometry.GetElementBoxAsync(element);
                if (IntersectsElement(_state.Location, updatedBoundingBox))
                {
                    return;
                }

                await MoveToAsync(
                    GetTargetPoint(updatedBoundingBox, options),
                    new PathOptions
                    {
                        MoveSpeed = options.MoveSpeed,
                        SpreadOverride = OvershootSpread
                    },
                    options.DelayPerStep);

                if (IntersectsElement(_state.Location, updatedBoundingBox))
                {
                    return;
                }
            }
            catch (Exception ex) when (canReacquire && attempt < options.MaxTries)
            {
                lastException = ex;
            }
        }

        if (lastException is not null)
        {
            throw new InvalidOperationException(
                "Could not move inside the element within the allowed number of attempts.",
                lastException);
        }

        throw new InvalidOperationException("Could not move inside the element within the allowed number of attempts.");
    }

    public Task MouseDownAsync(ResolvedClickOptions options)
        => _state.Page.Mouse.DownAsync(new PuppeteerSharp.Input.ClickOptions
        {
            Button = options.Button,
            Count = options.ClickCount
        });

    public Task MouseUpAsync(ResolvedClickOptions options)
        => _state.Page.Mouse.UpAsync(new PuppeteerSharp.Input.ClickOptions
        {
            Button = options.Button,
            Count = options.ClickCount
        });

    public async Task ClickAsync(ResolvedClickOptions options)
    {
        await GhostCursorTiming.DelayAsync(options.Hesitate);
        await MouseDownAsync(options);
        await GhostCursorTiming.DelayAsync(options.WaitForClick);
        await MouseUpAsync(options);
        await GhostCursorTiming.DelayAsync(options.MoveDelay, options.RandomizeMoveDelay);
    }

    private async Task MoveWithOvershootAsync(Vector destination, ResolvedMoveOptions options)
    {
        var basePathOptions = new PathOptions
        {
            MoveSpeed = options.MoveSpeed,
            SpreadOverride = options.SpreadOverride
        };

        if (ShouldOvershoot(_state.Location, destination, options.OvershootThreshold))
        {
            await MoveToAsync(
                Overshoot(destination, OvershootRadius),
                basePathOptions,
                options.DelayPerStep);
            await MoveToAsync(
                destination,
                new PathOptions
                {
                    MoveSpeed = options.MoveSpeed,
                    SpreadOverride = OvershootSpread
                },
                options.DelayPerStep);
            return;
        }

        await MoveToAsync(destination, basePathOptions, options.DelayPerStep);
    }

    private static Vector GetTargetPoint(BoundingBox boundingBox, ResolvedMoveOptions options)
        => CursorTargeting.GetPointInBox(boundingBox, new BoxOptions
        {
            PaddingPercentage = options.PaddingPercentage,
            Destination = options.Destination
        });

    private static bool IntersectsElement(Vector point, BoundingBox box)
        => point.X > Convert.ToDouble(box.X) &&
           point.X <= Convert.ToDouble(box.X + box.Width) &&
           point.Y > Convert.ToDouble(box.Y) &&
           point.Y <= Convert.ToDouble(box.Y + box.Height);

    private static bool ShouldOvershoot(Vector from, Vector to, double threshold)
        => Distance(from, to) > threshold;

    private static Vector Overshoot(Vector coordinate, double radius)
    {
        var angle = Random.Shared.NextDouble() * 2 * Math.PI;
        var randomRadius = radius * Math.Sqrt(Random.Shared.NextDouble());

        return new Vector(
            coordinate.X + (randomRadius * Math.Cos(angle)),
            coordinate.Y + (randomRadius * Math.Sin(angle)));
    }

    private static double Distance(Vector from, Vector to)
    {
        var deltaX = to.X - from.X;
        var deltaY = to.Y - from.Y;
        return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
    }
}
