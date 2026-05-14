# GhostCursorSharp

`GhostCursorSharp` is a .NET 10 port of the upstream [`ghost-cursor`](https://github.com/Xetera/ghost-cursor) package, built around `PuppeteerSharp` and `Microsoft.Playwright`.

It generates human-like mouse paths between coordinates and provides browser-facing cursor APIs for moving, clicking, scrolling, and debugging cursor movement across Puppeteer and Playwright pages.

## Installation

Install the package:

```powershell
dotnet add package Ruri.GhostCursorSharp
```

If you are developing this repo itself:

```powershell
dotnet restore GhostCursorSharp.slnx
```

## Status

Implemented today:
- Path generation with `CursorPath.Generate(...)` and `CursorPath.GenerateTimed(...)`
- `GhostCursor` and `PlaywrightGhostCursor` movement and click APIs
- Selector lookup with CSS and XPath
- `scroll`, `scrollTo`, and `scrollIntoView`
- Random movement with `PerformRandomMoves` and `ToggleRandomMove(...)`
- Public element-box geometry helper with inline-element and iframe-aware fallbacks
- Visual mouse helper installation
- Default action options and `CreateCursor(...)` compatibility shim

## Usage

Generate movement data between two coordinates:

```csharp
using GhostCursorSharp;

var from = new Vector(100, 100);
var to = new Vector(600, 700);

var route = CursorPath.Generate(from, to);
```

Generate movement data with timestamps:

```csharp
using GhostCursorSharp;

var route = CursorPath.GenerateTimed(
    new Vector(100, 100),
    new Vector(600, 700),
    new PathOptions
    {
        MoveSpeed = 10
    });
```

Use it with `PuppeteerSharp`:

```csharp
using GhostCursorSharp;
using PuppeteerSharp;

var browserFetcher = new BrowserFetcher();
var installedBrowser = browserFetcher.GetInstalledBrowsers().FirstOrDefault()
    ?? await browserFetcher.DownloadAsync();

await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = false,
    ExecutablePath = installedBrowser.GetExecutablePath()
});

await using var page = await browser.NewPageAsync();
await page.GoToAsync("https://example.com");

var cursor = new GhostCursor(page);

await cursor.ClickAsync("a");
```

Use it with `Microsoft.Playwright`:

```csharp
using GhostCursorSharp;
using Microsoft.Playwright;

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = false
});

var page = await browser.NewPageAsync();
await page.GotoAsync("https://example.com");

var cursor = new PlaywrightGhostCursor(page);

await cursor.ClickAsync("a");
```

Using default action options:

```csharp
using GhostCursorSharp;

var cursor = GhostCursor.CreateCursor(
    page,
    defaultOptions: new DefaultOptions
    {
        Click = new ClickOptions
        {
            WaitForSelector = 1000,
            MoveSpeed = 24,
            PaddingPercentage = 100
        },
        Scroll = new ScrollIntoViewOptions
        {
            ScrollSpeed = 100,
            ScrollDelay = 100
        }
    });

await cursor.ClickAsync("#submit");
await cursor.ScrollToAsync("bottom");
```

## Puppeteer Behavior

Current behavior mirrors the upstream package in the following areas:
- `MoveAsync(...)` targets a random point inside the element unless `Destination` is specified.
- `PaddingPercentage` controls how center-biased or edge-biased element targeting is.
- Long moves can overshoot and then correct back toward the destination.
- `ClickAsync(selector)` is shorthand for moving to the target and then clicking.
- `ScrollIntoViewAsync(...)` will bring offscreen elements into view before movement.
- Selector-based move and click retries can reacquire a rerendered target across attempts.

## API

### `new GhostCursor(page, start?)`

```csharp
var cursor = new GhostCursor(page);
var cursorWithStart = new GhostCursor(page, new Vector(100, 100));
```

Creates a cursor bound to a `PuppeteerSharp.IPage`.

- `page`: Puppeteer page instance.
- `start`: optional initial cursor position. Defaults to `(0, 0)`.

### `new GhostCursor(page, options)`

```csharp
var cursor = new GhostCursor(page, new GhostCursorOptions
{
    Start = new Vector(100, 100),
    Visible = true,
    DefaultOptions = new DefaultOptions()
});
```

- `Start`: optional initial cursor position.
- `Visible`: installs the mouse helper immediately.
- `DefaultOptions`: default options applied to actions like `click`, `move`, `scroll`, and `getElement`.

### `new PlaywrightGhostCursor(page, start?)`

```csharp
var cursor = new PlaywrightGhostCursor(page);
var cursorWithStart = new PlaywrightGhostCursor(page, new Vector(100, 100));
```

Creates a cursor bound to a `Microsoft.Playwright.IPage`.

### `new PlaywrightGhostCursor(page, options)`

```csharp
var cursor = new PlaywrightGhostCursor(page, new GhostCursorOptions
{
    Start = new Vector(100, 100),
    Visible = true,
    DefaultOptions = new DefaultOptions()
});
```

Supports the same option model as the Puppeteer facade.

### `GhostCursor.CreateCursor(page, start?, defaultOptions?, visible?)`

Compatibility-oriented factory:

```csharp
var cursor = GhostCursor.CreateCursor(page, visible: true);
```

### `PlaywrightGhostCursor.CreateCursor(page, start?, defaultOptions?, visible?)`

Compatibility-oriented factory for Playwright:

```csharp
var cursor = PlaywrightGhostCursor.CreateCursor(page, visible: true);
```

### `GetLocation(): Vector`

Returns the current tracked cursor position.

### `InstallMouseHelperAsync(): Task<MouseHelperInstallation>`

Installs a visible cursor overlay into the page for debugging.

### `RemoveMouseHelperAsync(): Task`

Removes the visible cursor overlay if installed.

### `GetElementAsync(selector, options?): Task<IElementHandle>`

Resolves an element by CSS selector or XPath.

- `selector`: CSS selector or XPath.
- `options.WaitForSelector`: optional timeout in milliseconds before failing.

### `GetElementAsync(element, options?): Task<IElementHandle>`

Returns an existing handle unchanged.

- `element`: existing `IElementHandle`.
- `options`: accepted for parity, but ignored when a handle is already provided.

### `GetElementBoxAsync(element, relativeToMainFrame?): Task<BoundingBox>`

Resolves an element box using the same geometry fallbacks as cursor movement.

- Uses `DOM.getContentQuads` when available.
- Falls back to `boundingBox()` and then `getBoundingClientRect()`.
- Handles inline elements more reliably than a plain bounding box.

### `MoveAsync(selector | element | boundingBox, options?): Task`

Moves the cursor to a target element or bounding box.

- `PaddingPercentage`: target more toward center or more toward edges.
- `Destination`: explicit point relative to the target's top-left corner.
- `MoveSpeed`: path density/speed hint.
- `MoveDelay`: delay after movement.
- `RandomizeMoveDelay`: randomize the final delay between `0` and `MoveDelay`.
- `WaitForSelector`: optional selector wait timeout.
- `ScrollSpeed`: scroll speed used if the element must be brought into view.
- `ScrollDelay`: delay after scrolling.
- `InViewportMargin`: extra margin when checking viewport visibility.
- `SpreadOverride`: override the path spread.
- `DelayPerStep`: extra C#-specific delay between generated mouse points.
- `MaxTries`: maximum number of reacquire/retry attempts if the element shifts or rerenders during movement.
- `OvershootThreshold`: distance threshold for overshoot correction.

Examples:

```csharp
await cursor.MoveAsync("#button");

await cursor.MoveAsync("#card", new MoveOptions
{
    PaddingPercentage = 0,
    MoveSpeed = 28
});

await cursor.MoveAsync("#card", new MoveOptions
{
    Destination = new Vector(20, 14)
});
```

### `MoveToAsync(destination, pathOptions?, delayPerStep?): Task`

Moves the cursor to an absolute page coordinate.

### `MoveByAsync(delta, pathOptions?, delayPerStep?): Task`

Moves the cursor by a relative offset.

### `ClickAsync(options?): Task`

Clicks at the current cursor location.

- `Hesitate`: delay before pressing the mouse button.
- `WaitForClick`: delay between mouse down and mouse up.
- `Button`: mouse button. Defaults to left.
- `ClickCount`: number of clicks. Defaults to `1`.
- Inherits the move-related options from `MoveOptions`.

### `ClickAsync(selector | element, options?): Task`

Moves to the target, then clicks it.

```csharp
await cursor.ClickAsync("#submit");

await cursor.ClickAsync("#submit", new ClickOptions
{
    WaitForSelector = 1000,
    PaddingPercentage = 100,
    MoveSpeed = 26
});
```

### `MouseDownAsync(options?): Task`

Presses a mouse button at the current cursor position.

### `MouseUpAsync(options?): Task`

Releases a mouse button at the current cursor position.

### `ScrollIntoViewAsync(selector | element, options?): Task`

Brings an element into view if needed.

- `ScrollSpeed`: `0-100`, where `100` is effectively instant.
- `ScrollDelay`: delay after scrolling.
- `InViewportMargin`: extra margin around the target element.
- `WaitForSelector`: optional selector wait timeout for string selectors.

### `ScrollAsync(delta, options?): Task`

Scrolls the page by a relative delta.

```csharp
await cursor.ScrollAsync(new Vector(0, 500));
```

### `ScrollToAsync(destination, options?): Task`

Scrolls to an absolute destination.

Supported overloads:
- `ScrollToAsync("top" | "bottom" | "left" | "right", options?)`
- `ScrollToAsync(new ScrollToDestination { X = ..., Y = ... }, options?)`

## Path API

### `CursorPath.Generate(start, end, options?): IReadOnlyList<Vector>`

Generates a human-like route between two coordinates or from a coordinate to a `BoundingBox`.

### `CursorPath.GenerateTimed(start, end, options?): IReadOnlyList<TimedVector>`

Same as `Generate(...)`, but includes timestamps.

### `PathOptions`

- `SpreadOverride`: override the Bezier spread.
- `MoveSpeed`: speed hint used during path generation.

## Debugging

Run the included demo:

```powershell
dotnet run --project .\src\GhostCursorSharp.Demo\GhostCursorSharp.Demo.csproj
```

That demo lets you choose `Puppeteer - Chromium`, `Playwright - Chromium`, or `Playwright - Firefox`, then runs scenario-driven tours for movement, clicking, press/release, scrolling, and random motion with the visual mouse helper enabled.

## Attribution

This project is a C# port of the original [`ghost-cursor`](https://github.com/Xetera/ghost-cursor) work by Xetera and contributors.
The upstream project is MIT licensed.
