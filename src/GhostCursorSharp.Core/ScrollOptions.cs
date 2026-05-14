namespace GhostCursorSharp;

/// <summary>
/// Configures page scrolling behavior.
/// </summary>
public class ScrollOptions
{
    /// <summary>
    /// Gets the scroll speed from <c>0</c> to <c>100</c>, where <c>100</c> is effectively instant.
    /// </summary>
    public double? ScrollSpeed { get; init; }

    /// <summary>
    /// Gets the delay in milliseconds to wait after scrolling.
    /// </summary>
    public int? ScrollDelay { get; init; }
}
