using System.Reflection;
using PuppeteerSharp;

namespace GhostCursorSharp.Internal;

internal sealed class PuppeteerCursorElementGeometry : ICursorElementGeometry
{
    private readonly IPage _page;

    public PuppeteerCursorElementGeometry(IPage page)
    {
        _page = page;
    }

    public async Task<ElementBox> GetElementBoxAsync(ICursorElementHandle element, bool relativeToMainFrame = true)
    {
        var nativeElement = UnwrapElement(element);

        try
        {
            var objectId = GetRemoteObjectId(nativeElement);
            if (string.IsNullOrWhiteSpace(objectId))
            {
                throw new InvalidOperationException("Element does not expose a CDP object id.");
            }

            var quads = await _page.Client.SendAsync<ContentQuadsResponse>(
                "DOM.getContentQuads",
                new
                {
                    objectId
                });

            var firstQuad = quads.Quads.FirstOrDefault();
            if (firstQuad is null || firstQuad.Length < 6)
            {
                throw new InvalidOperationException("Element content quads were not available.");
            }

            var elementBox = new ElementBox(
                firstQuad[0],
                firstQuad[1],
                firstQuad[4] - firstQuad[0],
                firstQuad[5] - firstQuad[1]);

            if (!relativeToMainFrame)
            {
                return await AdjustForChildFrameAsync(nativeElement, elementBox);
            }

            return elementBox;
        }
        catch
        {
            try
            {
                var boundingBox = await nativeElement.BoundingBoxAsync()
                    ?? throw new InvalidOperationException("Element bounding box was null.");

                return new ElementBox(
                    Convert.ToDouble(boundingBox.X),
                    Convert.ToDouble(boundingBox.Y),
                    Convert.ToDouble(boundingBox.Width),
                    Convert.ToDouble(boundingBox.Height));
            }
            catch
            {
                var box = await nativeElement.EvaluateFunctionAsync<DomRectBox>(
                    "(el) => { const rect = el.getBoundingClientRect(); return { x: rect.x, y: rect.y, width: rect.width, height: rect.height }; }");

                return new ElementBox(box.X, box.Y, box.Width, box.Height);
            }
        }
    }

    private static IElementHandle UnwrapElement(ICursorElementHandle element)
        => element is INativeCursorElementHandle<IElementHandle> nativeElement
            ? nativeElement.NativeElement
            : throw new InvalidOperationException(
                $"The {nameof(PuppeteerCursorElementGeometry)} requires a Puppeteer element handle adapter.");

    private static string? GetRemoteObjectId(IElementHandle element)
    {
        // HACK: PuppeteerSharp keeps the CDP RemoteObject surface non-public, but
        // upstream ghost-cursor relies on the underlying object id for DOM.getContentQuads.
        // Keep this reflection narrowly scoped here so the rest of the port can stay on
        // the public API and we still get inline-element geometry parity.
#pragma warning disable S3011
        var remoteObjectProperty = element.GetType().GetProperty(
            "RemoteObject",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var remoteObject = remoteObjectProperty?.GetValue(element);
        var objectIdProperty = remoteObject?.GetType().GetProperty(
            "ObjectId",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#pragma warning restore S3011

        return objectIdProperty?.GetValue(remoteObject) as string;
    }

    private static async Task<ElementBox> AdjustForChildFrameAsync(IElementHandle element, ElementBox elementBox)
    {
        var elementFrame = await element.ContentFrameAsync();
        var parentFrame = elementFrame?.ParentFrame;
        if (parentFrame is null)
        {
            return elementBox;
        }

        var iframes = await parentFrame.QuerySelectorAllAsync("xpath/.//iframe");
        foreach (var iframe in iframes)
        {
            if (await iframe.ContentFrameAsync() != elementFrame)
            {
                continue;
            }

            var frameBox = await iframe.BoundingBoxAsync();
            if (frameBox is null)
            {
                return elementBox;
            }

            return elementBox with
            {
                X = elementBox.X - Convert.ToDouble(frameBox.X),
                Y = elementBox.Y - Convert.ToDouble(frameBox.Y)
            };
        }

        return elementBox;
    }

    private sealed class ContentQuadsResponse
    {
        public double[][] Quads { get; init; } = [];
    }

    private sealed record DomRectBox(double X, double Y, double Width, double Height);
}
