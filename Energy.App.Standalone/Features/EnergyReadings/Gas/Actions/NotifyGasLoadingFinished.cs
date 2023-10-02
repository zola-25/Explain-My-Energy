using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;

public class NotifyGasLoadingFinished
{

    public string Message { get; }
    public bool Success { get; }

    public NotifyGasLoadingFinished(bool success, string message = null)
    {
        Success = success;
        Message = message;
    }
}