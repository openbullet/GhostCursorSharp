namespace GhostCursorSharp.Internal;

internal static class GhostCursorTiming
{
    public static Task DelayAsync(int milliseconds, bool randomize = false)
    {
        if (milliseconds < 1)
        {
            return Task.CompletedTask;
        }

        var effectiveDelay = randomize
            ? Random.Shared.Next(0, milliseconds + 1)
            : milliseconds;

        return Task.Delay(effectiveDelay);
    }
}
