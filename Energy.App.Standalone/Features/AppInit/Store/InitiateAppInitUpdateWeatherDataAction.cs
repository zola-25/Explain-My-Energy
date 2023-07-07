namespace Energy.App.Standalone.Features.AppInit.Store
{
    public class  InitiateAppInitUpdateWeatherDataAction
    {
        
        public bool CanUpdateWeatherData { get; }

        public InitiateAppInitUpdateWeatherDataAction(bool canUpdateWeatherData)
        {
            CanUpdateWeatherData = canUpdateWeatherData;
        }
    }
}