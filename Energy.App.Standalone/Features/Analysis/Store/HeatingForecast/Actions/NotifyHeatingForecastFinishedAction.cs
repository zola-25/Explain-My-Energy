namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;

public class NotifyHeatingForecastFinishedAction
{

    public NotifyHeatingForecastFinishedAction(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public bool Success { get; }
    public string Message { get; }
}
