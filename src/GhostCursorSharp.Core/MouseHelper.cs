using GhostCursorSharp.Internal;

namespace GhostCursorSharp;

/// <summary>
/// Installs a visual mouse helper into supported browser pages for debugging cursor movement.
/// </summary>
public static class MouseHelper
{
    /// <summary>
    /// Installs the visual mouse helper into the page for the current document and future navigations when supported.
    /// </summary>
    /// <typeparam name="TPage">The browser page type.</typeparam>
    /// <param name="page">The page to decorate with the visual mouse helper.</param>
    /// <returns>An installation handle that can remove the helper later.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException">Thrown when no browser-specific mouse helper installer is available.</exception>
    public static Task<MouseHelperInstallation> InstallAsync<TPage>(TPage page)
        where TPage : class
    {
        ArgumentNullException.ThrowIfNull(page);
        return MouseHelperInstallerRegistry.InstallAsync(page);
    }
}
