using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;
using Energy.App.Standalone.Features.Setup.Meter.Store;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas
{
    // Delete readings with subscribe
    // Tariffs
    // Weather Data status

    public class GasReadingsEffects
    {
        private readonly IEnergyReadingImporter _energyReadingImporter;
        private readonly ICostCalculator _energyCostCalculator;

        private readonly IState<MeterSetupState> _meterSetupState;
        private readonly IState<GasReadingsState> _gasReadingsState;

        public GasReadingsEffects(IEnergyReadingImporter energyReadingImporter,
            IState<GasReadingsState> gasReadingsState,
            IState<MeterSetupState> meterSetupState,
            ICostCalculator energyCostCalculator)
        {
            _energyReadingImporter = energyReadingImporter;
            _gasReadingsState = gasReadingsState;
            _meterSetupState = meterSetupState;
            _energyCostCalculator = energyCostCalculator;
        }




        [EffectMethod]
        public async Task ReloadGasReadingsAndCosts(GasReloadReadingsAndCostsAction loadReadingsAction, IDispatcher dispatcher)
        {
            try
            {
                var basicReadings = await _energyReadingImporter.ImportFromMoveInOrPreviousYear(MeterType.Gas);


                var costedReadings = await CalculateCostedReadings(basicReadings);
                dispatcher.Dispatch(new GasStoreReloadedReadingsAndCostsAction(basicReadings.ToImmutableList(), costedReadings));
                dispatcher.Dispatch(new NotifyGasStoreReady(basicReadings.Count, costedReadings.Count));

            }
            catch (Exception e)
            {
                Console.WriteLine("Error calculating Gas reading costs:");
                Console.WriteLine(e);

                dispatcher.Dispatch(new NotifyGasCostsCalculationFailedAction());

            }
        }

        [EffectMethod]
        public async Task ReloadGasCostsOnly(GasReloadCostsOnlyAction reloadCostsAction, IDispatcher dispatcher)
        {
            try
            {
                var basicReadings = _gasReadingsState.Value.BasicReadings;

                var costedReadings = await CalculateCostedReadings(basicReadings);
                dispatcher.Dispatch(new GasStoreReloadedCostsOnlysAction(costedReadings));

                dispatcher.Dispatch(new NotifyGasStoreReady(0, costedReadings.Count));

            }
            catch (Exception e)
            {
                Console.WriteLine("Error calculating Gas reading costs:");
                Console.WriteLine(e);

                dispatcher.Dispatch(new NotifyGasCostsCalculationFailedAction());

            }
        }

        [EffectMethod]
        public async Task UpdateGasReadingsAndReloadCosts(GasUpdateReadingsAndReloadCostsAction updateReadingsAction, IDispatcher dispatcher)
        {
            try
            {
                var basicReadings = await _energyReadingImporter.ImportFromDate(MeterType.Gas, updateReadingsAction.LastReading);
                dispatcher.Dispatch(new GasStoreUpdatedReadingsAction(basicReadings.ToImmutableList()));

                var costedReadings = await CalculateCostedReadings(basicReadings);
                dispatcher.Dispatch(new GasStoreReloadedCostsOnlysAction(costedReadings));

                dispatcher.Dispatch(new NotifyGasStoreReady(basicReadings.Count, costedReadings.Count));

            }
            catch (Exception e)
            {
                Console.WriteLine("Error calculating Gas reading costs:");
                Console.WriteLine(e);

                dispatcher.Dispatch(new NotifyGasCostsCalculationFailedAction());

            }
        }

        [EffectMethod]
        public Task DeleteGasReadings(GasInitiateDeleteReadingsAction initiateDeleteAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new GasExecuteDeleteReadingsAction());
            dispatcher.Dispatch(new NotifyGasReadingsDeletedAction());
            return Task.CompletedTask;
        }

        private async Task<ImmutableList<CostedReading>> CalculateCostedReadings(IReadOnlyCollection<BasicReading> basicReadings)
        {
            var costedReadings = _energyCostCalculator
                    .GetCostReadings(basicReadings,
                        _meterSetupState.Value[MeterType.Gas].TariffDetails).ToImmutableList();
            return costedReadings;
        }
    }
}