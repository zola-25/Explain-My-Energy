using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
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
        public async Task ReloadElectricityReadingsAndCosts(ElectricityReloadReadingsAndCostsAction loadReadingsAction, IDispatcher dispatcher)
        {
            var basicReadings = await _energyReadingImporter.ImportFromMoveInOrPreviousYear(MeterType.Electricity);
            try
            {
                var costedReadings = await CalculateCostedReadings(basicReadings);
                dispatcher.Dispatch(new ElectricityStoreReloadedReadingsAndCostsAction(basicReadings.ToImmutableList(), costedReadings));

                dispatcher.Dispatch(new NotifyElectricityStoreReady(basicReadings.Count, costedReadings.Count));

            }
            catch (Exception e)
            {
                Console.WriteLine("Error calculating Electricity reading costs:");
                Console.WriteLine(e);

                dispatcher.Dispatch(new NotifyElectricityCostsCalculationFailedAction());

            }

        }

        [EffectMethod]
        public async Task ReloadElectricityCostsOnly(ElectricityReloadCostsOnlyAction reloadCostsAction, IDispatcher dispatcher)
        {
            var basicReadings = _electricityReadingsState.Value.BasicReadings;
            try
            {
                var costedReadings = await CalculateCostedReadings(basicReadings);
                dispatcher.Dispatch(new ElectricityStoreReloadedCostsOnlysAction(costedReadings));

                dispatcher.Dispatch(new NotifyElectricityStoreReady(0, costedReadings.Count));

            }
            catch (Exception e)
            {
                Console.WriteLine("Error calculating Electricity reading costs:");
                Console.WriteLine(e);

                dispatcher.Dispatch(new NotifyElectricityCostsCalculationFailedAction());

            }
        }

        [EffectMethod]
        public async Task UpdateElectricityReadingsAndReloadCosts(ElectricityUpdateReadingsAndReloadCostsAction updateReadingsAction, IDispatcher dispatcher)
        {
            try
            {
                var basicReadingsToUpdate = await _energyReadingImporter.ImportFromDate(MeterType.Electricity, updateReadingsAction.LastBasicReading);

                dispatcher.Dispatch(new ElectricityStoreUpdatedReadingsAction(basicReadingsToUpdate.ToImmutableList()));

                var basicReadings = _electricityReadingsState.Value.BasicReadings;
                var costedReadings = await CalculateCostedReadings(basicReadings);
                dispatcher.Dispatch(new ElectricityStoreReloadedCostsOnlysAction(costedReadings));

                dispatcher.Dispatch(new NotifyElectricityStoreReady(basicReadingsToUpdate.Count, costedReadings.Count));

            }
            catch (Exception e)
            {
                Console.WriteLine("Error calculating Electricity reading costs:");
                Console.WriteLine(e);

                dispatcher.Dispatch(new NotifyElectricityCostsCalculationFailedAction());

            }
        }

        [EffectMethod]
        public Task DeleteElectricityReadings(ElectricityInitiateDeleteReadingsAction initiateDeleteAction, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ElectricityExecuteDeleteReadingsAction());
            dispatcher.Dispatch(new NotifyElectricityReadingsDeletedAction());
            return Task.CompletedTask;

        }
        private async Task<ImmutableList<CostedReading>> CalculateCostedReadings(IReadOnlyCollection<BasicReading> basicReadings)
        {
            var costedReadings = _energyCostCalculator
                    .GetCostReadings(basicReadings,
                        _electricityTariffsState.Value.TariffDetails).ToImmutableList();
            return costedReadings;
        }

    }


}
