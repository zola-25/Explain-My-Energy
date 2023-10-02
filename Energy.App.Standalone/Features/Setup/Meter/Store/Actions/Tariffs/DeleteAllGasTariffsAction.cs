using System.Collections.Immutable;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions.Tariffs;

public class DeleteAllGasTariffsAction
{
    [ReducerMethod(typeof(DeleteAllGasTariffsAction))]
    public static MeterSetupState Store(MeterSetupState state)
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