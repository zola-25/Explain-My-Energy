using System.Collections.Immutable;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;

namespace Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;

public class StoreHeatingForecastStateAction
{
    public ImmutableList<DailyCostedReading> ForecastDailyReadings { get; }
    public ImmutableList<TemperaturePoint> TemperaturePoints { get; }

    public StoreHeatingForecastStateAction(ImmutableList<DailyCostedReading> forecastDailyReadings, ImmutableList<TemperaturePoint> temperaturePoints)
    {
        ForecastDailyReadings = forecastDailyReadings;
        TemperaturePoints = temperaturePoints;
    }
}



