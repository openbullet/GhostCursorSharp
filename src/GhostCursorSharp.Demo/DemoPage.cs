using PuppeteerSharp;

namespace GhostCursorSharp.Demo;

internal static class DemoPage
{
    public static async Task LoadAsync(IPage page, string html)
    {
        await page.SetViewportAsync(new ViewPortOptions
        {
            Width = 1280,
            Height = 840
        });

        await page.SetContentAsync(html);
    }
}
