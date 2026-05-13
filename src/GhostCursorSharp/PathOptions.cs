namespace GhostCursorSharp;

/// <summary>
/// Configures path generation behavior.
/// </summary>
public sealed class PathOptions
{
    /// <summary>
    /// Gets the optional spread override used for Bezier anchor generation.
    /// </summary>
    public double? SpreadOverride { get; init; }

    /// <summary>
    /// Gets the optional movement speed hint used when calculating path density and timestamps.
    /// </summary>
    public double? MoveSpeed { get; init; }
}
