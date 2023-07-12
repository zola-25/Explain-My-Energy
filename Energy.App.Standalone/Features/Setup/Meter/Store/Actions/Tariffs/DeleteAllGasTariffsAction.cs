using System.Collections.Immutable;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions.Tariffs;

public class DeleteAllGasTariffsAction
{
    [ReducerMethod]
    public static MeterSetupState Store(MeterSetupState state, DeleteAllGasTariffsAction action)
    {
        return state with
        {
            GasMeter = state.GasMeter with
            {
                TariffDetails = ImmutableList<TariffDetailState>.Empty
            }
        };
    }

}