using Spectre.Console;

namespace GhostCursorSharp.Demo.Scenarios;

internal sealed class RandomMoveScenario : IDemoScenario
{
    public string Name => "Random Move Loop";

    public async Task RunAsync(DemoScenarioContext context)
    {
        await context.LoadPageAsync(DemoPages.Viewport);
        var cursor = context.Cursor;

        cursor.DefaultOptions = new DefaultOptions
        {
            RandomMove = new RandomMoveOptions
            {
                MoveSpeed = 95,
                MoveDelay = 120,
                RandomizeMoveDelay = false,
                DelayPerStep = 1
            }
        };

        cursor.ToggleRandomMove(true);

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Letting the cursor wander for 6 seconds...", async _ => await Task.Delay(6000));

        cursor.ToggleRandomMove(false);
        await DemoScenarioSupport.PauseAsync(300);
    }
}
