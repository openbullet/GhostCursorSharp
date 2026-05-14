namespace GhostCursorSharp.Demo.Scenarios;

internal sealed class ClickShowcaseScenario : IDemoScenario
{
    public string Name => "Click Showcase";

    public async Task RunAsync(DemoScenarioContext context)
    {
        await context.LoadPageAsync(DemoPages.Viewport);
        var cursor = context.Cursor;

        await cursor.ClickAsync("#click-button", DemoScenarioSupport.CreateClickOptions(28, 2, hesitation: 160));
        await DemoScenarioSupport.PauseAsync(260);

        await cursor.ClickAsync("#click-button-secondary", DemoScenarioSupport.CreateClickOptions(
            30,
            1,
            clickCount: 2,
            waitForClick: 70,
            destination: new Vector(154, 36)));

        await DemoScenarioSupport.PauseAsync(260);
        await cursor.ClickAsync("#target-b", DemoScenarioSupport.CreateClickOptions(30, 1, destination: new Vector(42, 30)));
    }
}
