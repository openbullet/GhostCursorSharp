namespace GhostCursorSharp.Demo.Scenarios;

internal sealed class FullFeatureTourScenario : IDemoScenario
{
    private readonly IDemoScenario[] _scenarios;

    public FullFeatureTourScenario(params IDemoScenario[] scenarios)
    {
        _scenarios = scenarios;
    }

    public string Name => "Full Feature Tour";

    public async Task RunAsync(DemoScenarioContext context)
    {
        for (var i = 0; i < _scenarios.Length; i++)
        {
            await _scenarios[i].RunAsync(context);

            if (i < _scenarios.Length - 1)
            {
                await DemoScenarioSupport.PauseAsync(220);
            }
        }
    }
}
