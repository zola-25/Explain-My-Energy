namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;

public class NotifyGasForecastResult
{
    public bool Success { get; }
    public string Message { get; }

    public NotifyGasForecastResult(bool success, string message = null)
    {
        Success = success;
        Message = message;
    }
}