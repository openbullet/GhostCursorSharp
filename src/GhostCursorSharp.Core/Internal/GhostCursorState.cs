namespace GhostCursorSharp.Internal;

internal sealed class GhostCursorState
{
    private int _actionDepth;

    public GhostCursorState(ICursorPageAdapter page, Vector start)
    {
        Page = page;
        Location = start;
    }

    public ICursorPageAdapter Page { get; }

    public Vector Location { get; set; }

    public bool IsActionActive => _actionDepth > 0;

    public IDisposable BeginAction()
    {
        _actionDepth++;
        return new ActionScope(this);
    }

    private void EndAction()
    {
        if (_actionDepth > 0)
        {
            _actionDepth--;
        }
    }

    private sealed class ActionScope : IDisposable
    {
        private GhostCursorState? _state;

        public ActionScope(GhostCursorState state)
        {
            _state = state;
        }

        public void Dispose()
        {
            _state?.EndAction();
            _state = null;
        }
    }
}
