using PuppeteerSharp;

namespace GhostCursorSharp;

/// <summary>
/// Represents an installed mouse helper that can be removed from a page.
/// </summary>
public sealed class MouseHelperInstallation : IAsyncDisposable
{
    private readonly IPage _page;
    private readonly string _scriptIdentifier;
    private bool _removed;

    internal MouseHelperInstallation(IPage page, string scriptIdentifier)
    {
        _page = page;
        _scriptIdentifier = scriptIdentifier;
    }

    /// <summary>
    /// Removes the mouse helper from the current page and future navigations.
    /// </summary>
    /// <returns>A task that completes when the helper has been removed.</returns>
    public async Task RemoveAsync()
    {
        if (_removed)
        {
            return;
        }

        _removed = true;

        await _page.EvaluateExpressionAsync("""
            (() => {
              window.__ghostCursorRemoveMouseHelper?.();
            })();
            """);

        await _page.RemoveScriptToEvaluateOnNewDocumentAsync(_scriptIdentifier);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
        => new(RemoveAsync());
}
