namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;

public class NotifyHeatingSetupFinishedAction
{
    public string Message { get; }
    public bool Success { get; }

    public NotifyHeatingSetupFinishedAction(bool success, string message)
    {
        Message = message;
        Success = success;
    }
}


