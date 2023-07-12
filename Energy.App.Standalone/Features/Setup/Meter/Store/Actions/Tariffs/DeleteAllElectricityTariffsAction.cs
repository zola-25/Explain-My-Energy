using System.Collections.Immutable;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions.Tariffs;

public class DeleteAllElectricityTariffsAction
{
    [ReducerMethod]
    public static MeterSetupState Store(MeterSetupState state, DeleteAllElectricityTariffsAction action)
    {
        return state with
        {
            ElectricityMeter = state.ElectricityMeter with
            {
                TariffDetails = ImmutableList<TariffDetailState>.Empty
            }
        };
    }

}