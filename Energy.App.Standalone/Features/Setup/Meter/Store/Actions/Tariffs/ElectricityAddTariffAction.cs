using Energy.App.Standalone.DTOs.Tariffs;
using Energy.App.Standalone.Extensions;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions.Tariffs;

public class ElectricityAddTariffAction
{

    public TariffDetail TariffDetail { get; }

    public ElectricityAddTariffAction(TariffDetail tariffDetail)
    {
        TariffDetail = tariffDetail;
    }

    [ReducerMethod]
    public static MeterSetupState Store(MeterSetupState state, ElectricityAddTariffAction action)
    {
        var newTariffState = action.TariffDetail.eMapToTariffState(addGuidForNewTariff: true);
        // insert the new tariff after the last DateAppliesFrom index
        var index = state.ElectricityMeter.TariffDetails.FindLastIndex(x => x.DateAppliesFrom < newTariffState.DateAppliesFrom);

        return state with
        {
            ElectricityMeter = state.ElectricityMeter with
            {
                TariffDetails = state.ElectricityMeter.TariffDetails.Insert(index + 1, newTariffState)
            }
        };
    }
}

// write the same classes for gas