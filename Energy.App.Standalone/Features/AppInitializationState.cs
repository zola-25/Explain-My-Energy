using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Weather.Store;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;
using static Energy.App.Standalone.Features.EnergyReadings.Store.GasReadingsReducers;

namespace Energy.App.Standalone.Features
{
    public record AppInitializationState
    {
        public bool WaitingForWeatherData { get; init; }
        public bool WaitingForGasReadingsData { get; init; }
        public bool WaitingForElectricityReadingsData { get; init; }
        public bool WaitingForLinearCoefficients { get; init; }

    }

    public class AppInitializationFeature : Feature<AppInitializationState>
    {
        public override string GetName()
        {
            return nameof(AppInitializationFeature);
        }

        protected override AppInitializationState GetInitialState()
        {
            return new AppInitializationState
            {
                WaitingForWeatherData = true,
                WaitingForGasReadingsData = true,
                WaitingForElectricityReadingsData = true,
                WaitingForLinearCoefficients = true
            };
        }
    }

    public static class AppInitializationReducers
    {
        [ReducerMethod]
        public static AppInitializationState ReduceWeatherDataLoadedAction(AppInitializationState state, NotifyWeatherReadingsLoadedAction action)
        {
            return state with { WaitingForWeatherData = false };
        }

        [ReducerMethod]
        public static AppInitializationState ReduceGasReadingsLoadedAction(AppInitializationState state, NotifyGasCostsCalculationCompletedAction action)
        {
            return state with { WaitingForGasReadingsData = false };
        }


        [ReducerMethod]
        public static AppInitializationState ReduceElectricityReadingsLoadedAction(AppInitializationState state, NotifyElectricityCostsCalculationCompletedAction action)
        {
            return state with { WaitingForElectricityReadingsData = false };
        }

        [ReducerMethod]
        public static AppInitializationState ReduceLinearCoefficientsLoadedAction(AppInitializationState state, NotifyLinearCoeffiecientsReadyAction action)
        {
            return state with { WaitingForLinearCoefficients = false };
        }


        [ReducerMethod]
        public static AppInitializationState ReduceWeatherUpdateLoadableAction(AppInitializationState state, AppInitializationUpdateWeatherDataLoadableAction action)
        {
            return state with { WaitingForWeatherData = action.CanUpdateWeatherData };
        }

        [ReducerMethod]
        public static AppInitializationState ReduceGasReadingsUpdateLoadableAction(AppInitializationState state, AppInitializationUpdateGasReadingsDataLoadableAction action)
        {
            return state with { WaitingForGasReadingsData = action.CanUpdateGasReadingsData };
        }
        [ReducerMethod]
        public static AppInitializationState ReduceElectricityReadingsUpdateLoadableAction(AppInitializationState state, AppInitializationUpdateElectricityReadingsDataLoadableAction action)
        {
            return state with { WaitingForElectricityReadingsData = action.CanUpdateElectricityReadingsData };
        }

        [ReducerMethod]
        public static AppInitializationState ReduceLinearCoefficientsUpdateLoadableAction(AppInitializationState state, AppInitializationUpdateLinearCoefficientsLoadableAction action)
        {
            return state with { WaitingForLinearCoefficients = action.CanUpdateLinearCoefficients };
        }
    }
}
