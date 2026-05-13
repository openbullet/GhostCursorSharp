namespace GhostCursorSharp;

/// <summary>
/// Configures scrolling an element into the viewport.
/// </summary>
public sealed class ScrollIntoViewOptions : ScrollOptions
{
    /// <summary>
    /// Gets the optional time to wait for the selector to appear in milliseconds.
    /// </summary>
    public int? WaitForSelector { get; init; }

    /// <summary>
    /// Gets the margin in pixels to preserve around the element when ensuring it is visible.
    /// </summary>
    public double? InViewportMargin { get; init; }
}
