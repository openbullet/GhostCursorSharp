namespace GhostCursorSharp;

/// <summary>
/// Represents a document scroll destination.
/// </summary>
public sealed class ScrollToDestination
{
    /// <summary>
    /// Gets the target horizontal scroll coordinate in document space.
    /// </summary>
    public double? X { get; init; }

    /// <summary>
    /// Gets the target vertical scroll coordinate in document space.
    /// </summary>
    public double? Y { get; init; }
}
