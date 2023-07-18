namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions
{
    public class NotifyElectricityForecastResult
    {
        public bool Success { get; }
        public string Message { get; }

        public NotifyElectricityForecastResult(bool success, string message = null)
        {
            Success = success;
            Message = message;
        }
    }
}
