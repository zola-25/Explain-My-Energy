namespace Energy.App.Standalone.Features.Setup.Weather.Store
{
    public class NotifyWeatherLoadingFinished
    {
        public string Message { get; }
        public bool Success { get; }

        public NotifyWeatherLoadingFinished(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}