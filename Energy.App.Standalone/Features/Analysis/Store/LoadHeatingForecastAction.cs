using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Store;

public class LoadHeatingForecastAction
{
    public decimal DegreeDifference { get; }

    public LoadHeatingForecastAction(decimal degreeDifference)
    {
        DegreeDifference = degreeDifference;
    }
}



