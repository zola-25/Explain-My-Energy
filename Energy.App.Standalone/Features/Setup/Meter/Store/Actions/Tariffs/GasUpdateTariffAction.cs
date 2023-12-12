using Energy.App.Standalone.DTOs.Tariffs;
using Energy.App.Standalone.Extensions;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions.Tariffs;

public class GasUpdateTariffAction
{

    public TariffDetail TariffDetail { get; }

    public GasUpdateTariffAction(TariffDetail tariffDetail)
    {
        TariffDetail = tariffDetail;
    }

    [ReducerMethod]
    public static MeterSetupState Store(MeterSetupState state, GasUpdateTariffAction action)
    {
        var updatedTariffState = action.TariffDetail.eMapToTariffState(addGuidForNewTariff: false);
        // insert the new tariff after the last DateAppliesFrom index
        int index = state.GasMeter.TariffDetails.FindIndex(c => c.GlobalId == updatedTariffState.GlobalId);
        var toUpdate = state.GasMeter.TariffDetails.ToList();
        toUpdate[index] = updatedTariffState;
        return state with
        {
            GasMeter = state.GasMeter with
            {
                TariffDetails = toUpdate.OrderByDescending(c => c.DateAppliesFrom).ToImmutableList()
            }
        };
    }
}