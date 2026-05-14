using PuppeteerSharp;
using PlaywrightBoundingBox = Microsoft.Playwright.ElementHandleBoundingBoxResult;

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
    public static Vector GetPointInBox(BoundingBox boundingBox, BoxOptions? options = null)
    {
        if (options?.Destination is { } destination)
        {
            return new Vector(
                Convert.ToDouble(boundingBox.X) + destination.X,
                Convert.ToDouble(boundingBox.Y) + destination.Y);
        }

        var x = Convert.ToDouble(boundingBox.X);
        var y = Convert.ToDouble(boundingBox.Y);
        var width = Convert.ToDouble(boundingBox.Width);
        var height = Convert.ToDouble(boundingBox.Height);

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

    /// <summary>
    /// Resolves a target point inside the provided Playwright bounding box.
    /// </summary>
    /// <param name="boundingBox">The Playwright bounding box to target.</param>
    /// <param name="options">Optional targeting settings.</param>
    /// <returns>A point inside the bounding box.</returns>
    public static Vector GetPointInBox(PlaywrightBoundingBox boundingBox, BoxOptions? options = null)
        => GetPointInBox(
            new BoundingBox
            {
                X = Convert.ToDecimal(boundingBox.X),
                Y = Convert.ToDecimal(boundingBox.Y),
                Width = Convert.ToDecimal(boundingBox.Width),
                Height = Convert.ToDecimal(boundingBox.Height)
            },
            options);
}
