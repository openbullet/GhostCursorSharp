namespace GhostCursorSharp.Internal;

internal interface ICursorPageAdapter
{
    string CreateXPathSelector(string selector);

    Task WaitForSelectorAsync(string selector, int timeoutMilliseconds);

    Task<ICursorElementHandle?> QuerySelectorAsync(string selector);

    Task<IReadOnlyList<ICursorElementHandle>> QuerySelectorAllAsync(string selector);

    Task MoveMouseAsync(double x, double y, int steps = 1);

    Task MouseWheelAsync(double deltaX, double deltaY);

    Task MouseDownAsync(MouseButton button, int clickCount);

    Task MouseUpAsync(MouseButton button, int clickCount);

    Task<T> EvaluateFunctionAsync<T>(string script, params object?[] args);

    Task EvaluateFunctionAsync(string script, params object?[] args);
}
