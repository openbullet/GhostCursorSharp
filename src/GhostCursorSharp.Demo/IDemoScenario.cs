namespace GhostCursorSharp.Demo;

internal interface IDemoScenario
{
    string Name { get; }

    Task RunAsync(DemoScenarioContext context);
}
