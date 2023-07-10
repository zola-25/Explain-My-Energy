using System.Collections.Immutable;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;

namespace Energy.App.Standalone.Features.Analysis.Store;

public class StoreHeatingForecastStateAction
{
    public ImmutableList<DailyCostedReading> ForecastDailyReadings { get; }
    public ImmutableList<TemperaturePoint> TemperatureIconPoints { get; }

    public StoreHeatingForecastStateAction(ImmutableList<DailyCostedReading> forecastDailyReadings, ImmutableList<TemperaturePoint> temperatureIconPoints)
    {
        this.ForecastDailyReadings = forecastDailyReadings;
        TemperatureIconPoints = temperatureIconPoints;
    }
}



