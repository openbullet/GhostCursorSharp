using PuppeteerSharp;

namespace GhostCursorSharp.Internal;

internal interface ICursorElementGeometry
{
    Task<BoundingBox> GetElementBoxAsync(ICursorElementHandle element, bool relativeToMainFrame = true);
}
