namespace GhostCursorSharp.Demo.Scenarios;

internal sealed class BezierMoveScenario : IDemoScenario
{
    public string Name => "Bezier Move Tour";

    public async Task RunAsync(DemoScenarioContext context)
    {
        await context.LoadPageAsync(DemoPages.Viewport);
        var cursor = context.Cursor;

        await cursor.MoveAsync("#target-a", DemoScenarioSupport.CreateMoveOptions(24, 3, paddingPercentage: 0));
        await DemoScenarioSupport.PauseAsync(180);
        await cursor.MoveAsync("#target-b", DemoScenarioSupport.CreateMoveOptions(28, 2, destination: new Vector(26, 20)));
        await DemoScenarioSupport.PauseAsync(180);
        await cursor.MoveAsync("#target-d", DemoScenarioSupport.CreateMoveOptions(28, 2, destination: new Vector(154, 36)));
        await DemoScenarioSupport.PauseAsync(180);
        await cursor.MoveAsync("#target-c", DemoScenarioSupport.CreateMoveOptions(26, 2, destination: new Vector(34, 118)));
        await DemoScenarioSupport.PauseAsync(180);
        await cursor.MoveAsync("#target-b", DemoScenarioSupport.CreateMoveOptions(30, 1, destination: new Vector(188, 110)));
        await DemoScenarioSupport.PauseAsync(140);
        await cursor.MoveAsync("#target-a", DemoScenarioSupport.CreateMoveOptions(32, 1, paddingPercentage: 35));
    }
}
