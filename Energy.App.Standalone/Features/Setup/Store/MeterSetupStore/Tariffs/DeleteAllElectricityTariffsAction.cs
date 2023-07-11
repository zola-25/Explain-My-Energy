using System.Collections.Immutable;
using Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.StateObjects;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.Tariffs;

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