namespace GhostCursorSharp.Internal;

internal interface ICursorElementGeometry
{
    Task<ElementBox> GetElementBoxAsync(ICursorElementHandle element, bool relativeToMainFrame = true);
}
