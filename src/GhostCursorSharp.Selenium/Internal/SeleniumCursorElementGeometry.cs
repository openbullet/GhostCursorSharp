using OpenQA.Selenium;

namespace GhostCursorSharp.Internal;

internal sealed class SeleniumCursorElementGeometry : ICursorElementGeometry
{
    public async Task<ElementBox> GetElementBoxAsync(ICursorElementHandle element, bool relativeToMainFrame = true)
    {
        try
        {
            var rect = await element.EvaluateFunctionAsync<DomRectBox>(
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

            return new ElementBox(rect.X, rect.Y, rect.Width, rect.Height);
        }
        catch
        {
            if (element is not INativeCursorElementHandle<IWebElement> nativeElement)
            {
                throw new InvalidOperationException(
                    $"The {nameof(SeleniumCursorElementGeometry)} requires a Selenium element handle adapter.");
            }

            var location = nativeElement.NativeElement.Location;
            var size = nativeElement.NativeElement.Size;
            return new ElementBox(location.X, location.Y, size.Width, size.Height);
        }
    }

    private sealed record DomRectBox(double X, double Y, double Width, double Height);
}
