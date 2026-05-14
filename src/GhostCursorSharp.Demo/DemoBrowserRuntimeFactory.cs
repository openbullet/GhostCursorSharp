namespace GhostCursorSharp.Demo;

internal static class DemoBrowserRuntimeFactory
{
    public static async Task<IDemoBrowserRuntime> CreateAsync(DemoBrowserTarget target)
        => target switch
        {
            DemoBrowserTarget.PuppeteerChromium => await PuppeteerDemoBrowserRuntime.CreateAsync(),
            DemoBrowserTarget.PlaywrightChromium => await PlaywrightDemoBrowserRuntime.CreateAsync(DemoBrowserTarget.PlaywrightChromium),
            DemoBrowserTarget.PlaywrightFirefox => await PlaywrightDemoBrowserRuntime.CreateAsync(DemoBrowserTarget.PlaywrightFirefox),
            DemoBrowserTarget.SeleniumChromium => await SeleniumDemoBrowserRuntime.CreateAsync(DemoBrowserTarget.SeleniumChromium),
            DemoBrowserTarget.SeleniumFirefox => await SeleniumDemoBrowserRuntime.CreateAsync(DemoBrowserTarget.SeleniumFirefox),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unsupported demo browser target.")
        };
}
