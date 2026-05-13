namespace GhostCursorSharp;

/// <summary>
/// Stores default options for cursor actions.
/// </summary>
public sealed class DefaultOptions
{
    /// <summary>
    /// Gets the default options for move actions targeting elements.
    /// </summary>
    public MoveOptions? Move { get; init; }

    /// <summary>
    /// Gets the default options for click actions.
    /// </summary>
    public ClickOptions? Click { get; init; }

    /// <summary>
    /// Gets the default options for scroll actions.
    /// </summary>
    public ScrollIntoViewOptions? Scroll { get; init; }

    /// <summary>
    /// Gets the default options for selector resolution.
    /// </summary>
    public GetElementOptions? GetElement { get; init; }
}
