using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;

public class StoreCoefficientsAction
{
    public decimal Gradient { get; }
    public decimal C { get; }
    public DateTime LastUpdatedReadingDate { get; }

    public MeterType HeatingMeterType { get; }

    public StoreCoefficientsAction(decimal gradient,
                                   decimal c,
                                   MeterType heatingMeterType,
                                   DateTime lastUpdatedReadingDate)
    {
        Gradient = gradient;
        C = c;
        LastUpdatedReadingDate = lastUpdatedReadingDate;
        HeatingMeterType = heatingMeterType;
    }
}
