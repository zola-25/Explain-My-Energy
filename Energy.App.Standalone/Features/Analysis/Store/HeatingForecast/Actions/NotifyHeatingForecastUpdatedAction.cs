using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
public class NotifyHeatingForecastUpdatedAction
{
    public decimal DegreeDifference { get; }

    public NotifyHeatingForecastUpdatedAction(decimal degreeDifference = 0)
    {
        DegreeDifference = degreeDifference;
    }
}


