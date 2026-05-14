using PlaywrightBoundingBox = Microsoft.Playwright.ElementHandleBoundingBoxResult;
using PlaywrightElementHandle = Microsoft.Playwright.IElementHandle;
using System.Text.Json;

namespace GhostCursorSharp.Internal;

internal sealed class PlaywrightCursorElementGeometry : ICursorElementGeometry
{
    public async Task<ElementBox> GetElementBoxAsync(ICursorElementHandle element, bool relativeToMainFrame = true)
    {
        var nativeElement = UnwrapElement(element);

        try
        {
            var rect = await nativeElement.EvaluateAsync<JsonElement>(
                """
                (el) => {
                  const clientRect = [...el.getClientRects()].find(r => r.width > 0 && r.height > 0)
                    ?? el.getBoundingClientRect();
                  return {
                    x: clientRect.x,
                    y: clientRect.y,
                    width: clientRect.width,
                    height: clientRect.height
                  };
                }
                """);

            var width = rect.GetProperty("width").GetDouble();
            var height = rect.GetProperty("height").GetDouble();
            if (width > 0 && height > 0)
            {
                var boundingBox = new ElementBox(
                    rect.GetProperty("x").GetDouble(),
                    rect.GetProperty("y").GetDouble(),
                    width,
                    height);

                if (relativeToMainFrame)
                {
                    return await AdjustToMainFrameAsync(nativeElement, boundingBox);
                }

                return boundingBox;
            }
        }
        catch
        {
            // Playwright may fail to resolve a box for detached or inline-fragment-heavy elements.
            // Fall back to DOM geometry below so the cursor can still target the element.
        }

        var box = await nativeElement.BoundingBoxAsync()
            ?? throw new InvalidOperationException("Element bounding box was null.");
        var fallbackBoundingBox = ToElementBox(box);
        if (!relativeToMainFrame)
        {
            return await AdjustForChildFrameAsync(nativeElement, fallbackBoundingBox);
        }

        return fallbackBoundingBox;
    }

    private static PlaywrightElementHandle UnwrapElement(ICursorElementHandle element)
        => element is INativeCursorElementHandle<PlaywrightElementHandle> nativeElement
            ? nativeElement.NativeElement
            : throw new InvalidOperationException(
                $"The {nameof(PlaywrightCursorElementGeometry)} requires a Playwright element handle adapter.");

    private static ElementBox ToElementBox(PlaywrightBoundingBox box)
        => new(box.X, box.Y, box.Width, box.Height);

    private static async Task<ElementBox> AdjustToMainFrameAsync(PlaywrightElementHandle element, ElementBox elementBox)
    {
        var frame = await element.OwnerFrameAsync();
        while (frame?.ParentFrame is not null)
        {
            var frameElement = await frame.FrameElementAsync();
            var frameBox = await frameElement.BoundingBoxAsync();
            if (frameBox is null)
            {
                return elementBox;
            }

            elementBox = elementBox with
            {
                X = elementBox.X + frameBox.X,
                Y = elementBox.Y + frameBox.Y
            };

            frame = frame.ParentFrame;
        }

        return elementBox;
    }

    private static async Task<ElementBox> AdjustForChildFrameAsync(PlaywrightElementHandle element, ElementBox elementBox)
    {
        var frame = await element.OwnerFrameAsync();
        while (frame?.ParentFrame is not null)
        {
            var frameElement = await frame.FrameElementAsync();
            var frameBox = await frameElement.BoundingBoxAsync();
            if (frameBox is null)
            {
                return elementBox;
            }

            elementBox = elementBox with
            {
                X = elementBox.X - frameBox.X,
                Y = elementBox.Y - frameBox.Y
            };

            frame = frame.ParentFrame;
        }

        return elementBox;
    }
}
