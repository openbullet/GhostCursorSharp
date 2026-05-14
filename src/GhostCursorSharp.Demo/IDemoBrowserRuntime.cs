namespace GhostCursorSharp.Demo;

internal interface IDemoBrowserRuntime : IAsyncDisposable
{
    IDemoCursor Cursor { get; }

    Task LoadPageAsync(string pageAssetName, string baseDirectory);
}
