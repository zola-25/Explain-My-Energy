namespace Energy.App.Standalone.Features
{
    public class AppInitializationUpdateLoadableDataAction
    {
        public bool CanUpdateWeatherData { get; }
        public bool CanUpdateGasReadingsData { get; }
        public bool CanUpdateElectricityReadingsData { get; }
        public bool CanUpdateLinearCoefficients { get; }

        public AppInitializationUpdateLoadableDataAction(bool canUpdateWeatherData,
                                                         bool canUpdateGasReadingsData,
                                                         bool canUpdateElectricityReadingsData,
                                                         bool canUpdateLinearCoefficients)
        {
            CanUpdateWeatherData = canUpdateWeatherData;
            CanUpdateGasReadingsData = canUpdateGasReadingsData;
            CanUpdateElectricityReadingsData = canUpdateElectricityReadingsData;
            CanUpdateLinearCoefficients = canUpdateLinearCoefficients;
        }
    }
    
}
