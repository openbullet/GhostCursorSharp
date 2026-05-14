namespace GhostCursorSharp;

/// <summary>
/// Configures selector resolution against the page.
/// </summary>
public sealed class GetElementOptions
{
    /// <summary>
    /// Gets the optional time to wait for the selector to appear in milliseconds.
    /// </summary>
    public int? WaitForSelector { get; init; }
}
