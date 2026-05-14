namespace GhostCursorSharp;

/// <summary>
/// Configures background random cursor movement.
/// </summary>
public sealed class RandomMoveOptions
{
    /// <summary>
    /// Gets the optional movement speed hint used when calculating path density.
    /// Higher values result in fewer intermediate points and faster movement.
    /// </summary>
    public double? MoveSpeed { get; init; }

    /// <summary>
    /// Gets the optional delay in milliseconds between random move cycles.
    /// </summary>
    public int? MoveDelay { get; init; }

    /// <summary>
    /// Gets a value indicating whether the cycle delay should be randomized from <c>0</c> to <see cref="MoveDelay"/>.
    /// </summary>
    public bool? RandomizeMoveDelay { get; init; }

    /// <summary>
    /// Gets the optional delay in milliseconds between generated path points.
    /// This is an extra C# convenience that does not exist in the upstream package.
    /// </summary>
    public int? DelayPerStep { get; init; }
}
