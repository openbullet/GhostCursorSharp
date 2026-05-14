using PlaywrightBoundingBox = Microsoft.Playwright.ElementHandleBoundingBoxResult;
using PlaywrightElementHandle = Microsoft.Playwright.IElementHandle;
using PuppeteerSharp;
using System.Text.Json;

namespace GhostCursorSharp.Internal;

internal sealed class PlaywrightCursorElementGeometry : ICursorElementGeometry
{
    public async Task<BoundingBox> GetElementBoxAsync(ICursorElementHandle element, bool relativeToMainFrame = true)
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
                var boundingBox = new BoundingBox
                {
                    X = Convert.ToDecimal(rect.GetProperty("x").GetDouble()),
                    Y = Convert.ToDecimal(rect.GetProperty("y").GetDouble()),
                    Width = Convert.ToDecimal(width),
                    Height = Convert.ToDecimal(height)
                };

                if (relativeToMainFrame)
                {
                    await AdjustToMainFrameAsync(nativeElement, boundingBox);
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
        var fallbackBoundingBox = ToPuppeteerBoundingBox(box);
        if (!relativeToMainFrame)
        {
            await AdjustForChildFrameAsync(nativeElement, fallbackBoundingBox);
        }

        return fallbackBoundingBox;
    }

    private static PlaywrightElementHandle UnwrapElement(ICursorElementHandle element)
        => element is INativeCursorElementHandle<PlaywrightElementHandle> nativeElement
            ? nativeElement.NativeElement
            : throw new InvalidOperationException(
                $"The {nameof(PlaywrightCursorElementGeometry)} requires a Playwright element handle adapter.");

    private static BoundingBox ToPuppeteerBoundingBox(PlaywrightBoundingBox box)
        => new()
        {
            X = Convert.ToDecimal(box.X),
            Y = Convert.ToDecimal(box.Y),
            Width = Convert.ToDecimal(box.Width),
            Height = Convert.ToDecimal(box.Height)
        };

    private static async Task AdjustToMainFrameAsync(PlaywrightElementHandle element, BoundingBox elementBox)
    {
        var frame = await element.OwnerFrameAsync();
        while (frame?.ParentFrame is not null)
        {
            var frameElement = await frame.FrameElementAsync();
            var frameBox = await frameElement.BoundingBoxAsync();
            if (frameBox is null)
            {
                return;
            }

            elementBox.X += Convert.ToDecimal(frameBox.X);
            elementBox.Y += Convert.ToDecimal(frameBox.Y);

            frame = frame.ParentFrame;
        }
    }

    private static async Task AdjustForChildFrameAsync(PlaywrightElementHandle element, BoundingBox elementBox)
    {
        var frame = await element.OwnerFrameAsync();
        while (frame?.ParentFrame is not null)
        {
            var frameElement = await frame.FrameElementAsync();
            var frameBox = await frameElement.BoundingBoxAsync();
            if (frameBox is null)
            {
                return;
            }

            elementBox.X -= Convert.ToDecimal(frameBox.X);
            elementBox.Y -= Convert.ToDecimal(frameBox.Y);

            frame = frame.ParentFrame;
        }
    }
}
