using PuppeteerSharp.Input;

namespace GhostCursorSharp.Internal;

internal sealed record ResolvedGetElementOptions(int? WaitForSelector);

internal record ResolvedScrollOptions(double ScrollSpeed, int ScrollDelay);

internal record ResolvedScrollIntoViewOptions(
    double ScrollSpeed,
    int ScrollDelay,
    int? WaitForSelector,
    double InViewportMargin)
    : ResolvedScrollOptions(ScrollSpeed, ScrollDelay);

internal record ResolvedMoveOptions(
    int? WaitForSelector,
    double ScrollSpeed,
    int ScrollDelay,
    double InViewportMargin,
    double? SpreadOverride,
    double? MoveSpeed,
    int MoveDelay,
    bool RandomizeMoveDelay,
    int DelayPerStep,
    int MaxTries,
    double OvershootThreshold,
    double? PaddingPercentage,
    Vector? Destination)
    : ResolvedScrollIntoViewOptions(ScrollSpeed, ScrollDelay, WaitForSelector, InViewportMargin);

internal sealed record ResolvedClickOptions(
    int? WaitForSelector,
    double ScrollSpeed,
    int ScrollDelay,
    double InViewportMargin,
    double? SpreadOverride,
    double? MoveSpeed,
    int MoveDelay,
    bool RandomizeMoveDelay,
    int DelayPerStep,
    int MaxTries,
    double OvershootThreshold,
    double? PaddingPercentage,
    Vector? Destination,
    int Hesitate,
    int WaitForClick,
    MouseButton Button,
    int ClickCount)
    : ResolvedMoveOptions(
        WaitForSelector,
        ScrollSpeed,
        ScrollDelay,
        InViewportMargin,
        SpreadOverride,
        MoveSpeed,
        MoveDelay,
        RandomizeMoveDelay,
        DelayPerStep,
        MaxTries,
        OvershootThreshold,
        PaddingPercentage,
        Destination);
