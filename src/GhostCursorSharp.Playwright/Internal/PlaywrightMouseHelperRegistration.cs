using System.Runtime.CompilerServices;
using Microsoft.Playwright;

namespace GhostCursorSharp.Internal;

internal static class PlaywrightMouseHelperRegistration
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Register()
        => MouseHelperInstallerRegistry.Register<IPage>(InstallAsync);

    private static async Task<MouseHelperInstallation> InstallAsync(IPage page)
    {
        await page.EvaluateAsync(MouseHelperScript.Value);

        return new MouseHelperInstallation(async () =>
        {
            await page.EvaluateAsync("""
                (() => {
                  window.__ghostCursorRemoveMouseHelper?.();
                })();
                """);
        });
    }
}
