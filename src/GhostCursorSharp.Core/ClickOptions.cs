namespace GhostCursorSharp;

/// <summary>
/// Configures cursor clicking behavior.
/// </summary>
public sealed class ClickOptions : MoveOptions
{
    /// <summary>
    /// Gets the delay in milliseconds before the click starts.
    /// </summary>
    public int? Hesitate { get; init; }

    /// <summary>
    /// Gets the delay in milliseconds between mouse down and mouse up.
    /// </summary>
    public int? WaitForClick { get; init; }

    /// <summary>
    /// Gets the mouse button used for the click.
    /// </summary>
    public MouseButton? Button { get; init; }

    /// <summary>
    /// Gets the number of clicks to perform.
    /// </summary>
    public int? ClickCount { get; init; }
}
