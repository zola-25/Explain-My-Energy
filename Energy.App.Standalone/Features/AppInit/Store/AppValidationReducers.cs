using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Fluxor;

namespace Energy.App.Standalone.Features.AppInit.Store;

public static class AppValidationReducers
{
    [ReducerMethod]
    public static AppValidationState OnInitializeAppReducer(AppValidationState state, InitializeAppAction action)
    {
        return state with
        {
            AppStarted = true
        };
    }

    //[ReducerMethod]
    //public static AppValidationState OnInitiateReloadWeatherData (AppValidationState state, InitiateWeatherReloadReadingsAction action)
    //{
    //    return state with
    //    {
    //        WeatherDataInitialized = false,
    //        WeatherDataInitializing = true,
    //        WeatherDataStatus = $"Loading for area {action.OutCode}"
    //    };
    //}

    //[ReducerMethod]
    //public static AppValidationState OnInitiateUpdateWeatherData (AppValidationState state, InitiateWeatherUpdateReadingsAction action)
    //{
    //    return state with
    //    {
    //        WeatherDataInitializing = true,
    //        WeatherDataInitialized = false,
    //        WeatherDataStatus = $"Updating for area {action.OutCode}"
    //    };
    //}

    [ReducerMethod]
    public static AppValidationState OnNotifyWeathReaingsReady(AppValidationState state, NotifyWeatherReadingsReadyAction action)
    {
        return state with
        {
            WeatherDataInitializing = false,
            WeatherDataInitialized = true,
            WeatherDataStatus = $"Weather Data Loaded"
        };
    }

    [ReducerMethod]
    public static AppValidationState OnElectricityUpdatingNotification(AppValidationState state, NotifyElectricityStoreUpdating action)
    {
        return state with
        {
            ElectricityDataInitializing = true,
            ElectricityDataInitialized = false
        };
    }

    [ReducerMethod]
    public static AppValidationState OnElectricityReadyNotification(AppValidationState state, NotifyElectricityStoreReady action)
    {
        return state with
        {
            ElectricityDataInitializing = false,
            ElectricityDataInitialized = true
        };
    }

    [ReducerMethod]
    public static AppValidationState OnGasUpdatingNotification(AppValidationState state, NotifyGasStoreUpdating action)
    {
        return state with
        {
            GasDataInitializing = true,
            GasDataInitialized = false
        };
    }

    [ReducerMethod]
    public static AppValidationState OnGasReadyNotification(AppValidationState state, NotifyGasStoreReady action)
    {
        return state with
        {
            GasDataInitializing = false,
            GasDataInitialized = true
        };
    }

    [ReducerMethod]
    public static AppValidationState OnHeatingForecastUpdating(AppValidationState state, NotifyHeatingForecastUpdatingAction action)
    {
        return state with
        {
            ForecastInitialized = false,
            ForecastInitializing = true
        };
    }



    [ReducerMethod]
    public static AppValidationState OnHeatingForecastReady(AppValidationState state, NotifyHeatingForecastReadyAction action)
    {
        return state with
        {
            ForecastInitializing = false,
            ForecastInitialized = true
        };
    }
}