namespace Energy.App.Standalone.Features.Weather.Store
{
    public class InitiateWeatherReloadReadingsAction
    {
        public string OutCode { get; }

        public InitiateWeatherReloadReadingsAction(string outCode)
        {
            OutCode = outCode;
        }
    }
}