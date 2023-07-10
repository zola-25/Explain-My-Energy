using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas
{
    // Delete readings with subscribe
    // Tariffs
    // Weather Data status

    public class GasReadingsEffects
    {
        private readonly IEnergyReadingImporter _energyReadingImporter;
        private readonly ICostCalculator _energyCostCalculator;

        IState<GasReadingsState> _gasReadingsState;
        IState<GasTariffsState> _gasTariffsState;

        public GasReadingsEffects(IEnergyReadingImporter energyReadingImporter,
            IState<GasReadingsState> gasReadingsState,
            IState<GasTariffsState> gasTariffsState,
            ICostCalculator energyCostCalculator)
        {
            _energyReadingImporter = energyReadingImporter;
            _gasReadingsState = gasReadingsState;
            _gasTariffsState = gasTariffsState;
            _energyCostCalculator = energyCostCalculator;
        }




        [EffectMethod]
        public async Task ReloadGasReadings(GasReloadReadingsAndCostsAction loadReadingsAction, IDispatcher dispatcher)
        {
            List<BasicReading> basicReadings = await _energyReadingImporter.ImportFromMoveInOrPreviousYear(MeterType.Gas);
            try
            {
                var costedReadings = await CalculateCostedReadings(basicReadings);
                dispatcher.Dispatch(new GasStoreReloadedReadingsAction(basicReadings.ToImmutableList(), costedReadings));
                dispatcher.Dispatch(new NotifyGasCostsCalculationCompletedAction());
                dispatcher.Dispatch(new UpdateCoeffsAndOrForecastsIfSignificantOrOutdated(costedReadings.Count, MeterType.Gas));
                dispatcher.Dispatch(new NotifyGasStoreReady());

            }
            catch (Exception e)
            {
                Console.WriteLine("Error calculating Gas reading costs:");
                Console.WriteLine(e);

                dispatcher.Dispatch(new NotifyGasCostsCalculationFailedAction());
                dispatcher.Dispatch(new NotifyGasCostsCalculationCompletedAction(calculationError: true));

            }
        }

        [EffectMethod]
        public async Task UpdateGasReadings(GasUpdateReadingsAndCostsAction updateReadingsAction, IDispatcher dispatcher)
        {
            try
            {
                List<BasicReading> basicReadings = await _energyReadingImporter.ImportFromDate(MeterType.Gas, updateReadingsAction.LastReading);

                var costedReadings = await CalculateCostedReadings(basicReadings);
                dispatcher.Dispatch(new GasStoreUpdatedReadingsAction(basicReadings.ToImmutableList(), costedReadings));
                dispatcher.Dispatch(new NotifyGasCostsCalculationCompletedAction());
                dispatcher.Dispatch(new UpdateCoeffsAndOrForecastsIfSignificantOrOutdated(costedReadings.Count, MeterType.Gas));

                dispatcher.Dispatch(new NotifyGasStoreReady());

            }
            catch (Exception e)
            {
                Console.WriteLine("Error calculating Gas reading costs:");
                Console.WriteLine(e);

                dispatcher.Dispatch(new NotifyGasCostsCalculationFailedAction());
                dispatcher.Dispatch(new NotifyGasCostsCalculationCompletedAction(calculationError: true));

            }
        }

        [EffectMethod]
        public Task DeleteGasReadings(GasInitiateDeleteReadingsAction initiateDeleteAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new GasExecuteDeleteReadingsAction());
            dispatcher.Dispatch(new NotifyGasReadingsDeletedAction());
            return Task.CompletedTask;
        }

        private async Task<ImmutableList<CstR>> CalculateCostedReadings(List<BasicReading> basicReadings)
        {
            var costedReadings = _energyCostCalculator
                    .GetCostReadings(basicReadings,
                        _gasTariffsState.Value.TariffDetails).ToImmutableList();
            return costedReadings;
        }
    }
}