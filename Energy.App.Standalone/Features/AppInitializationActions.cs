namespace Energy.App.Standalone.Features
{
    public class InitializeAppAction { }

    public class  AppInitializationUpdateWeatherDataLoadableAction
    {
        
        public bool CanUpdateWeatherData { get; }

        public AppInitializationUpdateWeatherDataLoadableAction(bool canUpdateWeatherData)
        {
            CanUpdateWeatherData = canUpdateWeatherData;
        }
    }

    public class AppInitializationUpdateGasReadingsDataLoadableAction
    {
        public bool CanUpdateGasReadingsData { get; }

        public AppInitializationUpdateGasReadingsDataLoadableAction(bool canUpdateGasReadingsData)
        {
            CanUpdateGasReadingsData = canUpdateGasReadingsData;
        }
    }

    public class AppInitializationUpdateElectricityReadingsDataLoadableAction
    {
        public bool CanUpdateElectricityReadingsData { get; }

        public AppInitializationUpdateElectricityReadingsDataLoadableAction(bool canUpdateElectricityReadingsData)
        {
            CanUpdateElectricityReadingsData = canUpdateElectricityReadingsData;
        }
    }

    public class AppInitializationUpdateLinearCoefficientsLoadableAction
    {
        public bool CanUpdateLinearCoefficients { get; }

        public AppInitializationUpdateLinearCoefficientsLoadableAction(bool canUpdateLinearCoefficients)
        {
            CanUpdateLinearCoefficients = canUpdateLinearCoefficients;
        }
    }

}
