using System.Runtime.CompilerServices;
using PuppeteerSharp;

namespace GhostCursorSharp.Internal;

internal static class PuppeteerMouseHelperRegistration
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Register()
        => MouseHelperInstallerRegistry.Register<IPage>(InstallAsync);

    private static async Task<MouseHelperInstallation> InstallAsync(IPage page)
    {
        var scriptIdentifier = await page.EvaluateExpressionOnNewDocumentAsync(MouseHelperScript.Value);
        await page.EvaluateExpressionAsync(MouseHelperScript.Value);

        return new MouseHelperInstallation(async () =>
        {
            await page.EvaluateExpressionAsync("""
                (() => {
                  window.__ghostCursorRemoveMouseHelper?.();
                })();
                """);

            await page.RemoveScriptToEvaluateOnNewDocumentAsync(scriptIdentifier.Identifier);
        });
    }
}
