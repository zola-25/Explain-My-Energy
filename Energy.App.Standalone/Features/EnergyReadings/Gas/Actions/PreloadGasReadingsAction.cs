using Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;

public class PreloadGasReadingsAction
{
    public DemoGasReadings DemoGasReadings { get; }

    public PreloadGasReadingsAction(DemoGasReadings demoGasReadings)
    {
        DemoGasReadings = demoGasReadings;
    }


    // effects method
    public class PreloadDemoDataEffects : Effect<PreloadGasReadingsAction> {
        
        public override Task HandleAsync(PreloadGasReadingsAction action, IDispatcher dispatcher)
        {
            var gasReadings = action.DemoGasReadings.BasicReadings
                .Where(c => c.Utc >= DateTime.UtcNow.AddYears(-1).AddDays(-30)).ToImmutableList();
    
            dispatcher.Dispatch(new StorePreloadedGasReadingsAction(gasReadings));
            return Task.CompletedTask;

        }
    }
}
