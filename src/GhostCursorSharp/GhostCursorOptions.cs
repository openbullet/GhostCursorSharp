namespace GhostCursorSharp;

/// <summary>
/// Configures a <see cref="GhostCursor"/> instance.
/// </summary>
public sealed class GhostCursorOptions
{
    /// <summary>
    /// Gets the initial cursor location. Defaults to the origin.
    /// </summary>
    public Vector? Start { get; init; }

    /// <summary>
    /// Gets the default action options for this cursor instance.
    /// </summary>
    public DefaultOptions? DefaultOptions { get; init; }

    /// <summary>
    /// Gets a value indicating whether the visual mouse helper should be installed immediately.
    /// </summary>
    public bool Visible { get; init; }
}
