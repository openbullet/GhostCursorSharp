using PuppeteerSharp;

namespace GhostCursorSharp.Internal;

internal sealed class GhostCursorElementLocator
{
    private readonly IPage _page;

    public GhostCursorElementLocator(IPage page)
    {
        _page = page;
    }

    public async Task<IElementHandle> GetElementAsync(string selector, ResolvedGetElementOptions options)
    {
        if (selector.StartsWith("//", StringComparison.Ordinal) ||
            selector.StartsWith("(//", StringComparison.Ordinal))
        {
            var xpathSelector = $"xpath/.{selector}";

            if (options.WaitForSelector is not null)
            {
                await _page.WaitForSelectorAsync(xpathSelector, new WaitForSelectorOptions
                {
                    Timeout = options.WaitForSelector.Value
                });
            }

            var elements = await _page.QuerySelectorAllAsync(xpathSelector);
            return elements.FirstOrDefault()
                ?? throw new InvalidOperationException(
                    $"Could not find element with selector '{selector}'. " +
                    "Specify WaitForSelector when the element is expected to appear later.");
        }

        if (options.WaitForSelector is not null)
        {
            await _page.WaitForSelectorAsync(selector, new WaitForSelectorOptions
            {
                Timeout = options.WaitForSelector.Value
            });
        }

        return await _page.QuerySelectorAsync(selector)
            ?? throw new InvalidOperationException(
                $"Could not find element with selector '{selector}'. " +
                "Specify WaitForSelector when the element is expected to appear later.");
    }
}
