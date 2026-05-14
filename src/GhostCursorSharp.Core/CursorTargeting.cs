namespace GhostCursorSharp;

/// <summary>
/// Resolves target points inside element bounding boxes using upstream ghost-cursor semantics.
/// </summary>
public static class CursorTargeting
{
    /// <summary>
    /// Resolves a target point inside the provided bounding box.
    /// </summary>
    /// <param name="boundingBox">The bounding box to target.</param>
    /// <param name="options">Optional targeting settings.</param>
    /// <returns>A point inside the bounding box.</returns>
    public static Vector GetPointInBox(ElementBox boundingBox, BoxOptions? options = null)
    {
        if (options?.Destination is { } destination)
        {
            return new Vector(
                boundingBox.X + destination.X,
                boundingBox.Y + destination.Y);
        }

        var x = boundingBox.X;
        var y = boundingBox.Y;
        var width = boundingBox.Width;
        var height = boundingBox.Height;

        var paddingWidth = 0d;
        var paddingHeight = 0d;

        if (options?.PaddingPercentage is > 0 and <= 100)
        {
            paddingWidth = width * options.PaddingPercentage.Value / 100;
            paddingHeight = height * options.PaddingPercentage.Value / 100;
        }

        return new Vector(
            x + (paddingWidth / 2) + (Random.Shared.NextDouble() * (width - paddingWidth)),
            y + (paddingHeight / 2) + (Random.Shared.NextDouble() * (height - paddingHeight)));
    }
}
