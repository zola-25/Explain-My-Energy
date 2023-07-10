using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Store;

public class NotifyHeatingForecastReadyAction
{
    public decimal DegreeDifference { get; }

    public NotifyHeatingForecastReadyAction(decimal degreeDifference)
    {
        DegreeDifference = degreeDifference;
    }
}



