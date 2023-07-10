using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Store
{
    public class ElectricityReadingsEffects
    {
        private readonly IEnergyReadingImporter _energyReadingImporter;
        private readonly ICostCalculator _energyCostCalculator;
        IState<ElectricityTariffsState> _electricityTariffsState;
        IState<ElectricityReadingsState> _electricityReadingsState;

        public ElectricityReadingsEffects(IEnergyReadingImporter energyReadingImporter, IState<ElectricityTariffsState> electricityTariffsState, ICostCalculator energyCostCalculator, IState<ElectricityReadingsState> electricityReadingsState)
        {
            _energyReadingImporter = energyReadingImporter;
            _electricityTariffsState = electricityTariffsState;
            _energyCostCalculator = energyCostCalculator;
            _electricityReadingsState = electricityReadingsState;
        }

        [EffectMethod]
        public async Task ReloadElectricityReadings(ElectricityReloadReadingsAndCostsAction loadReadingsAction, IDispatcher dispatcher)
        {
            List<BasicReading> basicReadings = await _energyReadingImporter.ImportFromMoveInOrPreviousYear(MeterType.Electricity);
            try
            {
                var costedReadings = await CalculateCostedReadings(basicReadings);
                dispatcher.Dispatch(new ElectricityStoreReloadedReadingsAction(costedReadings));
                dispatcher.Dispatch(new NotifyElectricityCostsCalculationCompletedAction());
                dispatcher.Dispatch(new UpdateCoeffsAndOrForecastsIfSignificantOrOutdated(costedReadings.Count, MeterType.Electricity));

                dispatcher.Dispatch(new NotifyElectricityStoreReady());

            }
            catch (Exception e)
            {
                Console.WriteLine("Error calculating Electricity reading costs:");
                Console.WriteLine(e);

                dispatcher.Dispatch(new NotifyElectricityCostsCalculationFailedAction());
                dispatcher.Dispatch(new NotifyElectricityCostsCalculationCompletedAction(failed: true));

            }

        }

        [EffectMethod]
        public async Task UpdateElectricityReadings(ElectricityUpdateReadingsAndCostsAction updateReadingsAction, IDispatcher dispatcher)
        {
            try
            {
                List<BasicReading> basicReadings = await _energyReadingImporter.ImportFromDate(MeterType.Electricity, updateReadingsAction.LastReading);

                var costedReadings = await CalculateCostedReadings(basicReadings);
                dispatcher.Dispatch(new ElectricityStoreUpdatedReadingsAction(costedReadings));
                dispatcher.Dispatch(new NotifyElectricityCostsCalculationCompletedAction());
                dispatcher.Dispatch(new UpdateCoeffsAndOrForecastsIfSignificantOrOutdated(costedReadings.Count, MeterType.Electricity));

                dispatcher.Dispatch(new NotifyElectricityStoreReady());

            }
            catch (Exception e)
            {
                Console.WriteLine("Error calculating Electricity reading costs:");
                Console.WriteLine(e);

                dispatcher.Dispatch(new NotifyElectricityCostsCalculationFailedAction());
                dispatcher.Dispatch(new NotifyElectricityCostsCalculationCompletedAction(failed: true));

            }


        }

        [EffectMethod]
        public Task DeleteElectricityReadings(ElectricityInitiateDeleteReadingsAction initiateDeleteAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ElectricityExecuteDeleteReadingsAction());
            dispatcher.Dispatch(new NotifyElectricityReadingsDeletedAction());
            return Task.CompletedTask;

        }
        private async Task<ImmutableList<CostedReading>> CalculateCostedReadings(List<BasicReading> basicReadings)
        {
            var costedReadings = _energyCostCalculator
                    .GetCostReadings(basicReadings,
                        _electricityTariffsState.Value.TariffDetails).ToImmutableList();
            return costedReadings;
        }

    }


}
