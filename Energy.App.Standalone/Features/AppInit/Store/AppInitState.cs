using Fluxor;

namespace Energy.App.Standalone.Features.AppInit.Store;

public record AppInitState
{
    
    public bool CanUpdateWeatherData { get; init; }
    public bool WeatherDataInitializing { get; init; }
    public bool WeatherDataInitialized { get; init; }

    public bool CanUpdateGasData { get; init; }
    public bool GasDataInitializing { get; init; }
    public bool GasDataInitialized { get; init; }

    public bool CanUpdateElectricityData { get; init; }
    public bool ElectricityDataInitializing { get; init; }
    public bool ElectricityDataInitialized { get; init; }
    
    
    public bool CanUpdateLinearCoefficients { get; init; }
    public bool LinearCoefficientsInitializing { get; init; }
    public bool LinearCoefficientsInitialized { get; init; }
    public bool AppStarted { get; init; }
}

public class AppInitFeature : Feature<AppInitState>
{
    public override string GetName()
    {
        return nameof(AppInitFeature);
    }

    protected override AppInitState GetInitialState()
    {
        return new AppInitState
        {
            AppStarted = false,
            CanUpdateWeatherData = false,
            WeatherDataInitializing = false,
            WeatherDataInitialized = false,
            
            CanUpdateGasData = false,
            GasDataInitializing = false,
            GasDataInitialized = false,
            
            CanUpdateElectricityData = false,
            ElectricityDataInitializing = false,
            ElectricityDataInitialized = false,
            
            CanUpdateLinearCoefficients = false,
            LinearCoefficientsInitializing = false,
            LinearCoefficientsInitialized = false
        };
    }
}