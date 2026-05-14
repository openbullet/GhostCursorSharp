namespace GhostCursorSharp;

/// <summary>
/// Configures how a target point is selected inside a bounding box.
/// </summary>
public class BoxOptions
{
    /// <summary>
    /// Gets the percentage of padding to add inside the element before choosing a random target point.
    /// A value of <c>0</c> allows the full element area, while <c>100</c> resolves to the center.
    /// </summary>
    public double? PaddingPercentage { get; init; }

    /// <summary>
    /// Gets the destination relative to the top-left corner of the element.
    /// When provided, <see cref="PaddingPercentage"/> is ignored.
    /// </summary>
    public Vector? Destination { get; init; }
}
