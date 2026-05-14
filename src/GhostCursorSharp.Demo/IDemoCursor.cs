namespace GhostCursorSharp.Demo;

internal interface IDemoCursor
{
    DefaultOptions? DefaultOptions { get; set; }

    Task MoveAsync(string selector, MoveOptions? options = null);

    Task ClickAsync(string selector, ClickOptions? options = null);

    Task MouseDownAsync(ClickOptions? options = null);

    Task MouseUpAsync(ClickOptions? options = null);

    Task ScrollAsync(Vector delta, ScrollOptions? options = null);

    Task ScrollToAsync(ScrollToDestination destination, ScrollOptions? options = null);

    void ToggleRandomMove(bool random);
}
