namespace GhostCursorSharp.Demo.Scenarios;

internal sealed class ScrollScenario : IDemoScenario
{
    public string Name => "Scroll And Offscreen Targets";

    public async Task RunAsync(DemoScenarioContext context)
    {
        await context.LoadPageAsync(DemoPages.Scroll);
        var cursor = context.Cursor;

        await cursor.ScrollAsync(new Vector(0, 820), new ScrollOptions
        {
            ScrollDelay = 180,
            ScrollSpeed = 28
        });

        await DemoScenarioSupport.PauseAsync(200);
        await cursor.MoveAsync("#target-e", DemoScenarioSupport.CreateMoveOptions(28, 2, destination: new Vector(70, 44)));
        await DemoScenarioSupport.PauseAsync(180);
        await cursor.ClickAsync("#target-e", DemoScenarioSupport.CreateClickOptions(28, 2, hesitation: 80, destination: new Vector(70, 44)));

        await DemoScenarioSupport.PauseAsync(220);
        await cursor.ScrollToAsync(new ScrollToDestination { X = 1480, Y = 1320 }, new ScrollOptions
        {
            ScrollDelay = 220,
            ScrollSpeed = 32
        });

        await DemoScenarioSupport.PauseAsync(220);
        await cursor.MoveAsync("#target-f", DemoScenarioSupport.CreateMoveOptions(30, 1, paddingPercentage: 20));
        await DemoScenarioSupport.PauseAsync(160);
        await cursor.ClickAsync("#target-f", DemoScenarioSupport.CreateClickOptions(30, 1, destination: new Vector(138, 138)));

        await DemoScenarioSupport.PauseAsync(200);
        await cursor.ScrollToAsync(new ScrollToDestination { X = 0, Y = 0 }, new ScrollOptions
        {
            ScrollDelay = 220,
            ScrollSpeed = 36
        });
    }
}
