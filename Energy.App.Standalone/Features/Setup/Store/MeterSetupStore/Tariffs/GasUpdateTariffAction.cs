using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Models.Tariffs;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.Tariffs;

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