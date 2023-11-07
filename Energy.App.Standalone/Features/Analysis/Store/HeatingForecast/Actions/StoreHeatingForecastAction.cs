using System.Collections.Immutable;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.ChartModels;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;

public class StoreHeatingForecastAction
{
    public ImmutableList<DailyCostedReading> ForecastDailyReadings { get; }
    public ImmutableList<TemperaturePoint> TemperaturePoints { get; }
    public DateTime LastUpdatedReadingDate { get; }

    public StoreHeatingForecastAction(ImmutableList<DailyCostedReading> forecastDailyReadings,
                                      ImmutableList<TemperaturePoint> temperaturePoints,
                                      DateTime lastUpdatedReadingDate)
    {
        ForecastDailyReadings = forecastDailyReadings;
        TemperaturePoints = temperaturePoints;
        LastUpdatedReadingDate = lastUpdatedReadingDate;
    }
}



