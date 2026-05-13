namespace GhostCursorSharp.Internal;

internal sealed class GhostCursorRandomMover
{
    private readonly GhostCursorMover _mover;
    private readonly GhostCursorOptionResolver _optionResolver;
    private readonly GhostCursorScroller _scroller;
    private readonly GhostCursorState _state;
    private bool _isEnabled;
    private bool _isLoopRunning;

    public GhostCursorRandomMover(
        GhostCursorState state,
        GhostCursorMover mover,
        GhostCursorScroller scroller,
        GhostCursorOptionResolver optionResolver)
    {
        _state = state;
        _mover = mover;
        _scroller = scroller;
        _optionResolver = optionResolver;
    }

    public void Toggle(bool enabled)
    {
        _isEnabled = enabled;

        if (enabled && !_isLoopRunning)
        {
            _isLoopRunning = true;
            _ = RunLoopAsync();
        }
    }

    private async Task RunLoopAsync()
    {
        try
        {
            while (true)
            {
                var options = _optionResolver.ResolveRandomMoveOptions(null);

                if (_isEnabled && !_state.IsActionActive)
                {
                    var destination = await _scroller.GetRandomViewportPointAsync();
                    await _mover.MoveToWithAbortAsync(
                        destination,
                        new PathOptions
                        {
                            MoveSpeed = options.MoveSpeed
                        },
                        options.DelayPerStep,
                        () => !_isEnabled || _state.IsActionActive);
                }

                await GhostCursorTiming.DelayAsync(options.MoveDelay, options.RandomizeMoveDelay);

                if (!_isEnabled && !_state.IsActionActive)
                {
                    await Task.Delay(10);
                }
            }
        }
        catch
        {
            _isLoopRunning = false;
        }
    }
}
