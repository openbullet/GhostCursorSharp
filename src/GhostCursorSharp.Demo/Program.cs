using GhostCursorSharp;
using GhostCursorSharp.Demo;
using GhostCursorSharp.Demo.Scenarios;
using PuppeteerSharp;
using Spectre.Console;

var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
{
    Path = Path.Combine(AppContext.BaseDirectory, ".browser")
});

AnsiConsole.MarkupLine("[grey]Preparing Chromium...[/]");

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
        "--window-size=1480,980"
    ]
});

var page = (await browser.PagesAsync()).FirstOrDefault() ?? await browser.NewPageAsync();
var context = new DemoScenarioContext(page, AppContext.BaseDirectory);

var scenarios = new IDemoScenario[]
{
    new BezierMoveScenario(),
    new ClickShowcaseScenario(),
    new MousePressScenario(),
    new ScrollScenario(),
    new RandomMoveScenario(),
    new FullFeatureTourScenario(
        new BezierMoveScenario(),
        new ClickShowcaseScenario(),
        new MousePressScenario(),
        new ScrollScenario(),
        new RandomMoveScenario())
};

var scenarioMap = scenarios.ToDictionary(s => s.Name, StringComparer.Ordinal);

while (true)
{
    var scenarioName = await AnsiConsole.PromptAsync(
        new SelectionPrompt<string>()
            .Title("[yellow]Choose a GhostCursorSharp demo scenario[/]")
            .PageSize(10)
            .AddChoices(scenarioMap.Keys.Append("Exit")));

    if (scenarioName == "Exit")
    {
        break;
    }

    AnsiConsole.MarkupLine($"[grey]Running:[/] [cyan]{scenarioName}[/]");
    try
    {
        await scenarioMap[scenarioName].RunAsync(context);
        AnsiConsole.MarkupLine("[green]Scenario complete.[/] Press any key to choose another one.");
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]Scenario failed:[/] {Markup.Escape(ex.Message)}");
        AnsiConsole.MarkupLine("[grey]Press any key to return to the menu.[/]");
    }

    Console.ReadKey(true);
}

await browser.CloseAsync();
