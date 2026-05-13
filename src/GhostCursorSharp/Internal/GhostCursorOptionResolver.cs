using PuppeteerSharp.Input;

namespace GhostCursorSharp.Internal;

internal sealed class GhostCursorOptionResolver
{
    private readonly Func<DefaultOptions?> _getDefaultOptions;

    public GhostCursorOptionResolver(Func<DefaultOptions?> getDefaultOptions)
    {
        _getDefaultOptions = getDefaultOptions;
    }

    public ResolvedGetElementOptions ResolveGetElementOptions(GetElementOptions? options)
    {
        var defaults = _getDefaultOptions();
        return new ResolvedGetElementOptions(
            options?.WaitForSelector ?? defaults?.GetElement?.WaitForSelector);
    }

    public ResolvedScrollOptions ResolveScrollOptions(ScrollOptions? options)
    {
        var defaults = _getDefaultOptions();
        return new ResolvedScrollOptions(
            options?.ScrollSpeed ?? defaults?.Scroll?.ScrollSpeed ?? 100,
            options?.ScrollDelay ?? defaults?.Scroll?.ScrollDelay ?? 200);
    }

    public ResolvedRandomMoveOptions ResolveRandomMoveOptions(RandomMoveOptions? options)
    {
        var defaults = _getDefaultOptions();
        var randomMoveDefaults = defaults?.RandomMove;

        return new ResolvedRandomMoveOptions(
            options?.MoveSpeed ?? randomMoveDefaults?.MoveSpeed,
            options?.MoveDelay ?? randomMoveDefaults?.MoveDelay ?? 2000,
            options?.RandomizeMoveDelay ?? randomMoveDefaults?.RandomizeMoveDelay ?? true,
            options?.DelayPerStep ?? randomMoveDefaults?.DelayPerStep ?? 0);
    }

    public ResolvedMoveToOptions ResolveMoveToOptions(MoveToOptions? options)
    {
        var defaults = _getDefaultOptions();
        var moveToDefaults = defaults?.MoveTo;

        return new ResolvedMoveToOptions(
            options?.SpreadOverride ?? moveToDefaults?.SpreadOverride,
            options?.MoveSpeed ?? moveToDefaults?.MoveSpeed,
            options?.MoveDelay ?? moveToDefaults?.MoveDelay ?? 0,
            options?.RandomizeMoveDelay ?? moveToDefaults?.RandomizeMoveDelay ?? true,
            options?.DelayPerStep ?? moveToDefaults?.DelayPerStep ?? 0);
    }

    public ResolvedScrollIntoViewOptions ResolveScrollIntoViewOptions(ScrollIntoViewOptions? options)
    {
        var defaults = _getDefaultOptions();
        return new ResolvedScrollIntoViewOptions(
            options?.ScrollSpeed ?? defaults?.Scroll?.ScrollSpeed ?? 100,
            options?.ScrollDelay ?? defaults?.Scroll?.ScrollDelay ?? 200,
            options?.WaitForSelector ?? defaults?.Scroll?.WaitForSelector,
            options?.InViewportMargin ?? defaults?.Scroll?.InViewportMargin ?? 0);
    }

    public ResolvedMoveOptions ResolveMoveOptions(MoveOptions? options)
    {
        var defaults = _getDefaultOptions();
        var moveDefaults = defaults?.Move;

        return new ResolvedMoveOptions(
            options?.WaitForSelector ?? moveDefaults?.WaitForSelector ?? defaults?.GetElement?.WaitForSelector,
            options?.ScrollSpeed ?? moveDefaults?.ScrollSpeed ?? defaults?.Scroll?.ScrollSpeed ?? 100,
            options?.ScrollDelay ?? moveDefaults?.ScrollDelay ?? defaults?.Scroll?.ScrollDelay ?? 200,
            options?.InViewportMargin ?? moveDefaults?.InViewportMargin ?? defaults?.Scroll?.InViewportMargin ?? 0,
            options?.SpreadOverride ?? moveDefaults?.SpreadOverride,
            options?.MoveSpeed ?? moveDefaults?.MoveSpeed,
            options?.MoveDelay ?? moveDefaults?.MoveDelay ?? 0,
            options?.RandomizeMoveDelay ?? moveDefaults?.RandomizeMoveDelay ?? true,
            options?.DelayPerStep ?? moveDefaults?.DelayPerStep ?? 0,
            options?.MaxTries ?? moveDefaults?.MaxTries ?? 10,
            options?.OvershootThreshold ?? moveDefaults?.OvershootThreshold ?? 500,
            options?.PaddingPercentage ?? moveDefaults?.PaddingPercentage,
            options?.Destination ?? moveDefaults?.Destination);
    }

    public ResolvedClickOptions ResolveClickOptions(ClickOptions? options)
    {
        var defaults = _getDefaultOptions();
        var clickDefaults = defaults?.Click;
        var moveDefaults = ResolveMoveOptions(options);

        return new ResolvedClickOptions(
            options?.WaitForSelector ?? clickDefaults?.WaitForSelector ?? moveDefaults.WaitForSelector,
            options?.ScrollSpeed ?? clickDefaults?.ScrollSpeed ?? moveDefaults.ScrollSpeed,
            options?.ScrollDelay ?? clickDefaults?.ScrollDelay ?? moveDefaults.ScrollDelay,
            options?.InViewportMargin ?? clickDefaults?.InViewportMargin ?? moveDefaults.InViewportMargin,
            options?.SpreadOverride ?? clickDefaults?.SpreadOverride ?? moveDefaults.SpreadOverride,
            options?.MoveSpeed ?? clickDefaults?.MoveSpeed ?? moveDefaults.MoveSpeed,
            options?.MoveDelay ?? clickDefaults?.MoveDelay ?? 2000,
            options?.RandomizeMoveDelay ?? clickDefaults?.RandomizeMoveDelay ?? true,
            options?.DelayPerStep ?? clickDefaults?.DelayPerStep ?? moveDefaults.DelayPerStep,
            options?.MaxTries ?? clickDefaults?.MaxTries ?? moveDefaults.MaxTries,
            options?.OvershootThreshold ?? clickDefaults?.OvershootThreshold ?? moveDefaults.OvershootThreshold,
            options?.PaddingPercentage ?? clickDefaults?.PaddingPercentage ?? moveDefaults.PaddingPercentage,
            options?.Destination ?? clickDefaults?.Destination ?? moveDefaults.Destination,
            options?.Hesitate ?? clickDefaults?.Hesitate ?? 0,
            options?.WaitForClick ?? clickDefaults?.WaitForClick ?? 0,
            options?.Button ?? clickDefaults?.Button ?? MouseButton.Left,
            options?.ClickCount ?? clickDefaults?.ClickCount ?? 1);
    }
}
