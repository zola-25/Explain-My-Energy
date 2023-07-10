using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;

public class LoadHeatingForecastAction
{
    public decimal DegreeDifference { get; }

    public LoadHeatingForecastAction(decimal degreeDifference = 0)
    {
        DegreeDifference = degreeDifference;
    }

}



