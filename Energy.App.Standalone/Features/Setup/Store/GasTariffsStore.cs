using Energy.App.Standalone.Data;
using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.App.Standalone.Models;
using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.Store
{
    [PersistState, PriorityLoad]

    public record GasTariffsState
    {
        public List<TariffDetailState> TariffDetails { get; init; }

    }

    public class GasTariffsFeature : Feature<GasTariffsState>
    {
        public override string GetName()
        {
            return nameof(GasTariffsFeature);
        }

        protected override GasTariffsState GetInitialState()
        {
            return new GasTariffsState
            {
                TariffDetails = new List<TariffDetailState>()
            };
        }
    }

    public class GasInitiateSetDefaultTariffsAction { }

    public class GasExecuteSetDefaultTariffsAction
    {
        public List<TariffDetailState> DefaultGasTariffs { get; }

        public GasExecuteSetDefaultTariffsAction(List<TariffDetailState> defaultGasTarifs)
        {
            DefaultGasTariffs = defaultGasTarifs;
        }
    }



    public static class GasTariffsReducers
    {
        [ReducerMethod]
        public static GasTariffsState OnExecuteSetDefaultTariffsAction(GasTariffsState state, GasExecuteSetDefaultTariffsAction action)
        {
            return state with
            {
                TariffDetails = action.DefaultGasTariffs,
            };
        }
    }

    // Delete readings with subscribe
    // Tariffs
    // Weather Data status

    public class GasTariffsEffects
    {
        [EffectMethod]
        public void ExecuteSetDefaultGasTariffs(GasInitiateSetDefaultTariffsAction initiateSetDefaultTariffsAction, IDispatcher dispatcher)
        {
            var defaultTariffs = DefaultTariffData.DefaultTariffs.Where(c => c.ExampleTariffType == ExampleTariffType.StandardFixedDaily
                && c.MeterType == MeterType.Gas).Select(c => new TariffDetailState
                {
                    GlobalId = Guid.NewGuid(),
                    PencePerKWh = c.PencePerKWh,
                    DailyStandingChargePence = c.DailyStandingChargePence,
                    DateAppliesFrom = c.DateAppliesFrom,
                    IsHourOfDayFixed = c.IsHourOfDayFixed,
                    HourOfDayPrices = c.DefaultHourOfDayPrices.Select(h => new HourOfDayPriceState()
                    {
                        HourOfDay = h.HourOfDay,
                        PencePerKWh = h.PencePerKWh
                    }).ToList(),
                }).ToList();

            dispatcher.Dispatch(new GasExecuteSetDefaultTariffsAction(defaultTariffs));

        }

    }


}
