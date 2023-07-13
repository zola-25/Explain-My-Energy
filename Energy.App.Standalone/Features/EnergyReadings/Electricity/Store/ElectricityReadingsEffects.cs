using Energy.App.Standalone.Data.EnergyReadings.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;
using Energy.App.Standalone.Features.Setup.Meter.Store;

namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Store
{
    public class ElectricityReadingsEffects
    {
        private readonly IEnergyReadingWorkerService _energyReadingWorkerService;
        private readonly ICostCalculator _energyCostCalculator;
        IState<MeterSetupState> _meterSetupState;
        IState<ElectricityReadingsState> _electricityReadingsState;

        public ElectricityReadingsEffects(IState<MeterSetupState> meterSetupState, ICostCalculator energyCostCalculator, IState<ElectricityReadingsState> electricityReadingsState, IEnergyReadingWorkerService energyReadingWorkerService)
        {
            _meterSetupState = meterSetupState;
            _energyCostCalculator = energyCostCalculator;
            _electricityReadingsState = electricityReadingsState;
            this._energyReadingWorkerService = energyReadingWorkerService;
        }

        [EffectMethod]
        public async Task ReloadElectricityReadingsAndCosts(ElectricityReloadReadingsAndCostsAction loadReadingsAction, IDispatcher dispatcher)
        {
            var basicReadings = await _energyReadingWorkerService.ImportFromMoveInOrPreviousYear(MeterType.Electricity);
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
                var basicReadingsToUpdate = await _energyReadingWorkerService.ImportFromDate(MeterType.Electricity, updateReadingsAction.LastBasicReading);

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
                        _meterSetupState.Value[MeterType.Electricity].TariffDetails).ToImmutableList();
            return costedReadings;
        }

    }


}
