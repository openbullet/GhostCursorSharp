namespace GhostCursorSharp.Demo.Scenarios;

internal sealed class MousePressScenario : IDemoScenario
{
    public string Name => "Mouse Down / Up";

    public async Task RunAsync(DemoScenarioContext context)
    {
        await context.LoadPageAsync(DemoPages.Viewport);
        var cursor = context.Cursor;

        await cursor.MoveAsync("#press-pad", DemoScenarioSupport.CreateMoveOptions(26, 2, paddingPercentage: 100));
        await DemoScenarioSupport.PauseAsync(180);

        await cursor.MouseDownAsync(new ClickOptions
        {
            Button = MouseButton.Left,
            ClickCount = 1
        });

        await DemoScenarioSupport.PauseAsync(700);

        await cursor.MouseUpAsync(new ClickOptions
        {
            Button = MouseButton.Left,
            ClickCount = 1
        });

        await DemoScenarioSupport.PauseAsync(300);
        await cursor.ClickAsync("#press-pad", DemoScenarioSupport.CreateClickOptions(26, 2, hesitation: 0, destination: new Vector(110, 110)));
    }
}
