using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Weather.Store;
using Fluxor;

namespace Energy.App.Standalone.Features.AppInit.Store;

public static class AppInitReducers
{
    [ReducerMethod]
    public static AppInitState OnInitializeAppReducer(AppInitState state, InitializeAppAction action)
    {
        return state with
        {
            AppStarted = true
        };
    }
    
    [ReducerMethod]
    public static AppInitState OnInitiateUpdateWeatherDataReducer (AppInitState state, InitiateAppInitUpdateWeatherDataAction action)
    {
        return state with
        {
            CanUpdateWeatherData = action.CanUpdateWeatherData,
            WeatherDataInitializing = action.CanUpdateWeatherData
        };
    }

    [ReducerMethod]
    public static AppInitState OnInitiateUpdateGasReadingsReducer (AppInitState state, InitiateAppInitUpdateGasReadingsAction action)
    {
        return state with
        {
            CanUpdateGasData = action.CanUpdateGasReadingsData,
            GasDataInitializing = action.CanUpdateGasReadingsData
        };
    }

    [ReducerMethod]
    public static AppInitState OnInitiateUpdateElectricityReadingsReducer(AppInitState state, InititateAppInitUpdateElectricityReadingsAction action)
    {
        return state with
        {
            CanUpdateElectricityData = action.CanUpdateElectricityReadingsData,
            ElectricityDataInitializing = action.CanUpdateElectricityReadingsData
        };
    }

    [ReducerMethod]
    public static AppInitState OnInitiateUpdateLinearCoefficientsReducer(AppInitState state, InitiateAppInitUpdateLinearCoefficientsAction action)
    {
        return state with
        {
            CanUpdateLinearCoefficients = action.CanUpdateLinearCoefficients,
            LinearCoefficientsInitializing = action.CanUpdateLinearCoefficients
        };
    }

    [ReducerMethod]
    public static AppInitState OnNotifyWeatherDataReadyReducer(AppInitState state, NotifyWeatherReadingsReadyAction action)
    {
        return state with
        {
            WeatherDataInitializing = false,
            WeatherDataInitialized = true
        };
    }

    [ReducerMethod]
    public static AppInitState OnNotifyGasCostsCalculationCompletedReducer (AppInitState state, NotifyGasCostsCalculationCompletedAction action)
    {
        return state with
        {
            GasDataInitializing = false,
            GasDataInitialized = true
        };
    }


    [ReducerMethod]
    public static AppInitState OnNotifyElectricityCostsCalculationCompletedReducer (AppInitState state, NotifyElectricityCostsCalculationCompletedAction action)
    {
        return state with
        {
            ElectricityDataInitializing = false,
            ElectricityDataInitialized = true
        };
    }

    [ReducerMethod]
    public static AppInitState OnNotifyLinearCoefficientsReadyReducer (AppInitState state, NotifyLinearCoefficientsReadyAction action)
    {
        return state with
        {
            LinearCoefficientsInitializing = false,
            LinearCoefficientsInitialized = true
        };
    }
}