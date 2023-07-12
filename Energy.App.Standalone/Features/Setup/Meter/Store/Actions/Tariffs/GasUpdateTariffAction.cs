using Energy.App.Standalone.DTOs.Tariffs;
using Energy.App.Standalone.Extensions;
using Fluxor;

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
        var newTariffState = action.TariffDetail.eMapToTariffState(addGuidForNewTariff: false);
        // insert the new tariff after the last DateAppliesFrom index
        int index = state.GasMeter.TariffDetails.FindIndex(c => c.GlobalId == newTariffState.GlobalId);

        return state with
        {
            GasMeter = state.GasMeter with
            {
                TariffDetails = state.GasMeter.TariffDetails.SetItem(index, newTariffState)
            }
        };
    }
}