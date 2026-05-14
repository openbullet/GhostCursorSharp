namespace GhostCursorSharp.Demo;

internal static class DemoScenarioSupport
{
    public static MoveOptions CreateMoveOptions(
        double moveSpeed,
        int delayPerStep,
        double? paddingPercentage = null,
        Vector? destination = null)
        => new()
        {
            MoveSpeed = moveSpeed,
            DelayPerStep = delayPerStep,
            MoveDelay = 0,
            RandomizeMoveDelay = false,
            ScrollDelay = 80,
            ScrollSpeed = 100,
            PaddingPercentage = paddingPercentage,
            Destination = destination
        };

    public static ClickOptions CreateClickOptions(
        double moveSpeed,
        int delayPerStep,
        int clickCount = 1,
        int hesitation = 110,
        int waitForClick = 45,
        Vector? destination = null)
        => new()
        {
            MoveSpeed = moveSpeed,
            DelayPerStep = delayPerStep,
            MoveDelay = 0,
            RandomizeMoveDelay = false,
            ScrollDelay = 80,
            ScrollSpeed = 100,
            Hesitate = hesitation,
            WaitForClick = waitForClick,
            ClickCount = clickCount,
            PaddingPercentage = destination is null ? 24 : null,
            Destination = destination
        };

    public static Task PauseAsync(int milliseconds)
        => Task.Delay(milliseconds);
}
