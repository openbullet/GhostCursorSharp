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

    public async Task<BoundingBox> GetElementBoxAsync(ICursorElementHandle element, bool relativeToMainFrame = true)
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

            var elementBox = new BoundingBox
            {
                X = Convert.ToDecimal(firstQuad[0]),
                Y = Convert.ToDecimal(firstQuad[1]),
                Width = Convert.ToDecimal(firstQuad[4] - firstQuad[0]),
                Height = Convert.ToDecimal(firstQuad[5] - firstQuad[1])
            };

            if (!relativeToMainFrame)
            {
                await AdjustForChildFrameAsync(nativeElement, elementBox);
            }

            return elementBox;
        }
        catch
        {
            try
            {
                return await nativeElement.BoundingBoxAsync()
                    ?? throw new InvalidOperationException("Element bounding box was null.");
            }
            catch
            {
                var box = await nativeElement.EvaluateFunctionAsync<DomRectBox>(
                    "(el) => { const rect = el.getBoundingClientRect(); return { x: rect.x, y: rect.y, width: rect.width, height: rect.height }; }");

                return new BoundingBox
                {
                    X = Convert.ToDecimal(box.X),
                    Y = Convert.ToDecimal(box.Y),
                    Width = Convert.ToDecimal(box.Width),
                    Height = Convert.ToDecimal(box.Height)
                };
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

    private static async Task AdjustForChildFrameAsync(IElementHandle element, BoundingBox elementBox)
    {
        var elementFrame = await element.ContentFrameAsync();
        var parentFrame = elementFrame?.ParentFrame;
        if (parentFrame is null)
        {
            return;
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
                return;
            }

            elementBox.X -= frameBox.X;
            elementBox.Y -= frameBox.Y;
            return;
        }
    }

    private sealed class ContentQuadsResponse
    {
        public double[][] Quads { get; init; } = [];
    }

    private sealed record DomRectBox(double X, double Y, double Width, double Height);
}
