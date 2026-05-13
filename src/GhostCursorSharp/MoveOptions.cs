namespace GhostCursorSharp;

/// <summary>
/// Configures cursor movement toward an element target.
/// </summary>
public sealed class MoveOptions : BoxOptions
{
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
    /// Gets the optional delay in milliseconds between path points.
    /// </summary>
    public int DelayPerStep { get; init; }
}
