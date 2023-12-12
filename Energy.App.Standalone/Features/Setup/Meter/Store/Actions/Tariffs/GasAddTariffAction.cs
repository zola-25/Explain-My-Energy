using Energy.App.Standalone.DTOs.Tariffs;
using Energy.App.Standalone.Extensions;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Setup.Meter.Store.Actions.Tariffs;

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

        var tariffsByDate = state.GasMeter.TariffDetails.Append(newTariffState).OrderByDescending(c => c.DateAppliesFrom.Value).ToImmutableList();

        return state with
        {
            GasMeter = state.GasMeter with
            {
                TariffDetails = tariffsByDate
            }
        };
    }
}