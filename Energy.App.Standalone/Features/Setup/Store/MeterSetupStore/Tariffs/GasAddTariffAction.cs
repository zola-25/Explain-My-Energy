using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Models.Tariffs;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.Tariffs;

public class GasAddTariffAction
{

    public TariffDetail TariffDetail { get; }

    public GasAddTariffAction(TariffDetail tariffDetail)
    {
        TariffDetail = tariffDetail;
    }

    [ReducerMethod]
    public static MeterSetupState Store(MeterSetupState state, GasAddTariffAction action)
    {
        var newTariffState = action.TariffDetail.eMapToTariffState(addGuidForNewTariff: true);
        // insert the new tariff after the last DateAppliesFrom index
        var index = state.GasMeter.TariffDetails.FindLastIndex(x => x.DateAppliesFrom < newTariffState.DateAppliesFrom);

        return state with
        {
            GasMeter = state.GasMeter with
            {
                TariffDetails = state.GasMeter.TariffDetails.Insert(index + 1, newTariffState)
            }
        };
    }
}