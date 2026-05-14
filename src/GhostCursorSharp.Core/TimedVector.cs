namespace GhostCursorSharp;

/// <summary>
/// Represents a 2D point associated with a movement timestamp.
/// </summary>
public readonly record struct TimedVector
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimedVector"/> struct.
    /// </summary>
    /// <param name="x">The horizontal coordinate.</param>
    /// <param name="y">The vertical coordinate.</param>
    /// <param name="timestamp">The Unix timestamp in milliseconds for this point.</param>
    public TimedVector(double x, double y, long timestamp)
    {
        X = x;
        Y = y;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Gets the horizontal coordinate.
    /// </summary>
    public double X { get; }

    /// <summary>
    /// Gets the vertical coordinate.
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// Gets the Unix timestamp in milliseconds associated with this point.
    /// </summary>
    public long Timestamp { get; }
}
