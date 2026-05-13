using GhostCursorSharp;
using PuppeteerSharp;

var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
{
    Path = Path.Combine(AppContext.BaseDirectory, ".browser")
});

Console.WriteLine("Preparing Chromium...");

var installedBrowser = browserFetcher.GetInstalledBrowsers().FirstOrDefault()
    ?? await browserFetcher.DownloadAsync();

await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = false,
    ExecutablePath = installedBrowser.GetExecutablePath(),
    DefaultViewport = null,
    Args =
    [
        "--force-device-scale-factor=1",
        "--window-size=1440,960"
    ]
});

await using var page = await browser.NewPageAsync();
var cursor = new GhostCursor(page, new Vector(140, 140));
var demoPagePath = Path.Combine(AppContext.BaseDirectory, "demo-page.html");
var demoPageHtml = await File.ReadAllTextAsync(demoPagePath);

await page.SetContentAsync(demoPageHtml);

await cursor.InstallMouseHelperAsync();
await page.BringToFrontAsync();
await cursor.MoveAsync("#target-a", new MoveOptions
{
    MoveSpeed = 24,
    DelayPerStep = 3,
    PaddingPercentage = 0
});
await Task.Delay(180);

await cursor.MoveAsync("#target-b", new MoveOptions
{
    MoveSpeed = 28,
    DelayPerStep = 2,
    Destination = new Vector(18, 22)
});
await Task.Delay(180);

await cursor.MoveAsync("#target-d", new MoveOptions
{
    MoveSpeed = 28,
    DelayPerStep = 2,
    Destination = new Vector(176, 28)
});
await Task.Delay(180);

await cursor.MoveAsync("#target-c", new MoveOptions
{
    MoveSpeed = 26,
    DelayPerStep = 2,
    Destination = new Vector(26, 112)
});
await Task.Delay(180);

await cursor.MoveAsync("#target-b", new MoveOptions
{
    MoveSpeed = 30,
    DelayPerStep = 1,
    Destination = new Vector(212, 132)
});
await Task.Delay(140);

await cursor.MoveAsync("#target-a", new MoveOptions
{
    MoveSpeed = 32,
    DelayPerStep = 1,
    PaddingPercentage = 35
});

Console.WriteLine("Demo completed. Press Enter to close the browser.");
Console.ReadLine();
