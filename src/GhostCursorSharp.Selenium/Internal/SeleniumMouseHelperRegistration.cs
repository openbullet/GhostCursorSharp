using System.Runtime.CompilerServices;
using OpenQA.Selenium;

namespace GhostCursorSharp.Internal;

internal static class SeleniumMouseHelperRegistration
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Register()
        => MouseHelperInstallerRegistry.Register<IWebDriver>(InstallAsync);

    private static async Task<MouseHelperInstallation> InstallAsync(IWebDriver driver)
    {
        var javascriptExecutor = driver as IJavaScriptExecutor
            ?? throw new InvalidOperationException("The Selenium driver must implement IJavaScriptExecutor.");

        await SeleniumScriptExecutor.EvaluateExpressionAsync(javascriptExecutor, MouseHelperScript.Value);

        return new MouseHelperInstallation(async () =>
        {
            await SeleniumScriptExecutor.EvaluateExpressionAsync(
                javascriptExecutor,
                """
                (() => {
                  window.__ghostCursorRemoveMouseHelper?.();
                })();
                """);
        });
    }
}
