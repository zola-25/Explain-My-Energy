using Fluxor;

internal class SetupMiddleware : Middleware
{
    readonly IDemoDataProvider _demoDataProvider;

    public SetupMiddleware(IDemoDataProvider demoDataProvider)
    {
        _demoDataProvider = demoDataProvider;
    }

    public override async Task InitializeAsync(IDispatcher dispatcher, IStore store)
    {
        var presetState = await _demoDataProvider.GetDemoData();
        store.Features["DemoFeature"].RestoreState(presetState);
    }

}

internal interface IDemoDataProvider
{
    Task<object>  GetDemoData();
}