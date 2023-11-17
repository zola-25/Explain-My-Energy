using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;

public class PreloadElectricityReadingsAction
{
    public DemoElectricityReadings DemoElectricityReadings { get; }

    public PreloadElectricityReadingsAction(DemoElectricityReadings demoElectricityReadings)
    {
        DemoElectricityReadings = demoElectricityReadings;
    }

    public class PreloadDemoDataEffects : Effect<PreloadElectricityReadingsAction>
    {

        public override Task HandleAsync(PreloadElectricityReadingsAction action, IDispatcher dispatcher)
        {
            var gasReadings = action.DemoElectricityReadings.BasicReadings
                .Where(c => c.Utc >= DateTime.UtcNow.AddYears(-1).AddDays(-30))
                .ToImmutableList();

            dispatcher.Dispatch(new StorePreloadedElectricityReadingsAction(gasReadings));
            return Task.CompletedTask;
        }
    }
}
