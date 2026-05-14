namespace GhostCursorSharp;

/// <summary>
/// Represents a point or displacement on a 2D plane.
/// </summary>
public readonly record struct Vector
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector"/> struct.
    /// </summary>
    /// <param name="x">The horizontal coordinate.</param>
    /// <param name="y">The vertical coordinate.</param>
    public Vector(double x, double y)
    {
        X = x;
        Y = y;
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
    /// Gets the origin point <c>(0, 0)</c>.
    /// </summary>
    public static readonly Vector Origin = new(0, 0);
}
