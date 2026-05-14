namespace GhostCursorSharp.Tests;

internal static class BrowserTestSessionFactory
{
    public static async Task<IBrowserTestSession> CreateAsync(BrowserTestCase browserTestCase)
        => browserTestCase switch
        {
            BrowserTestCase.PuppeteerChromium => await PuppeteerBrowserTestSession.CreateAsync(),
            BrowserTestCase.PlaywrightChromium => await PlaywrightBrowserTestSession.CreateAsync(BrowserTestCase.PlaywrightChromium),
            BrowserTestCase.PlaywrightFirefox => await PlaywrightBrowserTestSession.CreateAsync(BrowserTestCase.PlaywrightFirefox),
            BrowserTestCase.SeleniumChromium => await SeleniumBrowserTestSession.CreateAsync(BrowserTestCase.SeleniumChromium),
            BrowserTestCase.SeleniumFirefox => await SeleniumBrowserTestSession.CreateAsync(BrowserTestCase.SeleniumFirefox),
            _ => throw new ArgumentOutOfRangeException(nameof(browserTestCase), browserTestCase, "Unsupported browser test case.")
        };
}
