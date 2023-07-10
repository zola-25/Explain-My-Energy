using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;

public class NotifyHeatingForecastReadyAction
{
    public decimal DegreeDifference { get; }

    public NotifyHeatingForecastReadyAction(decimal degreeDifference = 0)
    {
        DegreeDifference = degreeDifference;
    }
}


