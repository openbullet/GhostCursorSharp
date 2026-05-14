namespace GhostCursorSharp.Demo;

internal sealed class DemoScenarioContext
{
    private readonly string _baseDirectory;
    private readonly IDemoBrowserRuntime _runtime;

    public DemoScenarioContext(IDemoBrowserRuntime runtime, string baseDirectory)
    {
        _runtime = runtime;
        _baseDirectory = baseDirectory;
    }

    public IDemoCursor Cursor => _runtime.Cursor;

    public async Task LoadPageAsync(string pageAssetName)
        => await _runtime.LoadPageAsync(pageAssetName, _baseDirectory);
}
