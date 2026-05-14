using PuppeteerSharp;

namespace GhostCursorSharp.Demo;

internal sealed class DemoScenarioContext
{
    private readonly IPage _page;
    private readonly string _baseDirectory;
    private GhostCursor? _cursor;

    public DemoScenarioContext(IPage page, string baseDirectory)
    {
        _page = page;
        _baseDirectory = baseDirectory;
    }

    public IPage Page => _page;

    public GhostCursor Cursor
        => _cursor ?? throw new InvalidOperationException("The demo page must be loaded before accessing the cursor.");

    public async Task LoadPageAsync(string pageAssetName)
    {
        var pagePath = Path.Combine(_baseDirectory, pageAssetName);
        var html = await File.ReadAllTextAsync(pagePath);

        await DemoPage.LoadAsync(_page, html);

        _cursor = new GhostCursor(_page, new Vector(140, 140));
        await _cursor.InstallMouseHelperAsync();
        await _page.BringToFrontAsync();
    }
}
