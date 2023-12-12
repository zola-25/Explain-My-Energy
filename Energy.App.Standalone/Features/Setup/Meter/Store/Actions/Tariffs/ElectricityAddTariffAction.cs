using Energy.App.Standalone.DTOs.Tariffs;
using Energy.App.Standalone.Extensions;
using Fluxor;
using System.Collections.Immutable;

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

        var tariffsByDate = state.ElectricityMeter.TariffDetails.Append(newTariffState).OrderByDescending(c => c.DateAppliesFrom.Value).ToImmutableList();
        
        return state with
        {
            ElectricityMeter = state.ElectricityMeter with
            {
                TariffDetails = tariffsByDate
            }
        };
    }
}

