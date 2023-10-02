namespace Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;

public class NotifyElectricityLoadingFinished
{

    public bool Success { get; }
    public string Message { get; }

    public NotifyElectricityLoadingFinished(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}
