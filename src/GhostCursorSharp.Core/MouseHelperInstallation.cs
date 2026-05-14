namespace GhostCursorSharp;

/// <summary>
/// Represents an installed mouse helper that can be removed from a page.
/// </summary>
public sealed class MouseHelperInstallation : IAsyncDisposable
{
    private readonly Func<Task> _remove;
    private bool _removed;

    internal MouseHelperInstallation(Func<Task> remove)
    {
        _remove = remove;
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
        await _remove();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
        => new(RemoveAsync());
}
