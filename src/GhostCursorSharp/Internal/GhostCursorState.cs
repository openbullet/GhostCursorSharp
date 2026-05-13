using PuppeteerSharp;

namespace GhostCursorSharp.Internal;

internal sealed class GhostCursorState
{
    public GhostCursorState(IPage page, Vector start)
    {
        Page = page;
        Location = start;
    }

    public IPage Page { get; }

    public Vector Location { get; set; }
}
