using Fluxor;

namespace Energy.App.Standalone.Features.AppInit.Store;

public record AppValidationState
{
    public bool AppStarted { get; init; }
    
    public bool WeatherDataInitializing { get; init; }
    public bool WeatherDataInitialized { get; init; }
    public string WeatherDataStatus { get; init; }

    public bool GasDataInitializing { get; init; }
    public bool GasDataInitialized { get; init; }

    public bool ElectricityDataInitializing { get; init; }
    public bool ElectricityDataInitialized { get; init; }
    
    
    public bool ForecastInitializing { get; init; }
    public bool ForecastInitialized { get; init; }

    public bool IsLoading => WeatherDataInitializing || GasDataInitializing || ElectricityDataInitializing || ForecastInitializing;
}

public class AppInitFeature : Feature<AppValidationState>
{
    public override string GetName()
    {
        return nameof(AppInitFeature);
    }

    protected override AppValidationState GetInitialState()
    {
        return new AppValidationState
        {
            AppStarted = false,
            WeatherDataInitializing = false,
            WeatherDataInitialized = false,
            
            GasDataInitializing = false,
            GasDataInitialized = false,
            
            ElectricityDataInitializing = false,
            ElectricityDataInitialized = false,
            
            ForecastInitializing = false,
            ForecastInitialized = false
        };
    }
}