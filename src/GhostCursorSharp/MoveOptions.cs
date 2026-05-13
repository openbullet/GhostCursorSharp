namespace GhostCursorSharp;

/// <summary>
/// Configures cursor movement toward an element target.
/// </summary>
public class MoveOptions : BoxOptions
{
    /// <summary>
    /// Gets the optional time to wait for the selector to appear in milliseconds.
    /// </summary>
    public int? WaitForSelector { get; init; }

    /// <summary>
    /// Gets the optional scroll speed from <c>0</c> to <c>100</c> used when the cursor must bring an element into view.
    /// </summary>
    public double? ScrollSpeed { get; init; }

    /// <summary>
    /// Gets the optional delay in milliseconds to wait after scrolling an element into view.
    /// </summary>
    public int? ScrollDelay { get; init; }

    /// <summary>
    /// Gets the optional margin in pixels to preserve around an element when ensuring it is visible.
    /// </summary>
    public double? InViewportMargin { get; init; }

    /// <summary>
    /// Gets the optional Bezier spread override used during path generation.
    /// </summary>
    public double? SpreadOverride { get; init; }

    /// <summary>
    /// Gets the optional movement speed hint used when calculating path density.
    /// Higher values result in fewer intermediate points and faster movement.
    /// </summary>
    public double? MoveSpeed { get; init; }

    /// <summary>
    /// Gets the optional delay in milliseconds after moving.
    /// </summary>
    public int? MoveDelay { get; init; }

    /// <summary>
    /// Gets a value indicating whether the action delay should be randomized from <c>0</c> to <see cref="MoveDelay"/>.
    /// </summary>
    public bool? RandomizeMoveDelay { get; init; }

    /// <summary>
    /// Gets the optional delay in milliseconds between generated path points.
    /// This is an extra C# convenience that does not exist in the upstream package.
    /// </summary>
    public int? DelayPerStep { get; init; }

    /// <summary>
    /// Gets the maximum number of attempts to move inside an element if it shifts during movement.
    /// </summary>
    public int? MaxTries { get; init; }

    /// <summary>
    /// Gets the distance from the current location that triggers an overshoot and correction pass.
    /// </summary>
    public double? OvershootThreshold { get; init; }
}
