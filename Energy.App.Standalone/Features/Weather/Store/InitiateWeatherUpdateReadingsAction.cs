namespace Energy.App.Standalone.Features.Weather.Store
{
    public class InitiateWeatherUpdateReadingsAction
    {
        public string OutCode { get; }

        public DateTime? LatestReading { get; }
        public DateTime? LatestHistorical { get; }

        public InitiateWeatherUpdateReadingsAction(string outCode,
            DateTime? latestReading,
            DateTime? latestHistorical)
        {
            OutCode = outCode;
            LatestReading = latestReading;
            LatestHistorical = latestHistorical;
        }
    }
}