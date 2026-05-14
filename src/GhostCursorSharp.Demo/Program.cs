using GhostCursorSharp;
using GhostCursorSharp.Demo;
using GhostCursorSharp.Demo.Scenarios;
using Spectre.Console;

var scenarios = new IDemoScenario[]
{
    new BezierMoveScenario(),
    new ClickShowcaseScenario(),
    new MousePressScenario(),
    new RandomMoveScenario(),
    new ScrollScenario(),
    new FullFeatureTourScenario(
        new BezierMoveScenario(),
        new ClickShowcaseScenario(),
        new MousePressScenario(),
        new RandomMoveScenario(),
        new ScrollScenario())
};

var scenarioMap = scenarios.ToDictionary(s => s.Name, StringComparer.Ordinal);
var browserChoices = new Dictionary<string, DemoBrowserTarget>(StringComparer.Ordinal)
{
    ["Puppeteer - Chromium"] = DemoBrowserTarget.PuppeteerChromium,
    ["Playwright - Chromium"] = DemoBrowserTarget.PlaywrightChromium,
    ["Playwright - Firefox"] = DemoBrowserTarget.PlaywrightFirefox,
    ["Selenium - Chromium"] = DemoBrowserTarget.SeleniumChromium
};

while (true)
{
    var browserChoice = await AnsiConsole.PromptAsync(
        new SelectionPrompt<string>()
            .Title("[yellow]Choose a browser engine for the GhostCursorSharp demo[/]")
            .PageSize(10)
            .AddChoices(browserChoices.Keys.Append("Exit")));

    if (browserChoice == "Exit")
    {
        break;
    }

    await using var runtime = await DemoBrowserRuntimeFactory.CreateAsync(browserChoices[browserChoice]);
    var context = new DemoScenarioContext(runtime, AppContext.BaseDirectory);

    while (true)
    {
        var scenarioName = await AnsiConsole.PromptAsync(
            new SelectionPrompt<string>()
                .Title($"[yellow]{browserChoice}[/] [grey]- choose a GhostCursorSharp demo scenario[/]")
                .PageSize(10)
                .AddChoices(scenarioMap.Keys.Append("Change Browser").Append("Exit")));

        if (scenarioName == "Exit")
        {
            return;
        }

        if (scenarioName == "Change Browser")
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
}
