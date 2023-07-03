using Energy.App.Standalone.Features.Analysis.Store;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public interface IForecastGenerator
    {
        ImmutableList<BasicReading> GetBasicReadingsForecast(DateTime start, DateTime end, decimal degreeDifference, LinearCoefficientsState linearCoefficientsState, ImmutableList<DailyWeatherReading> dailyWeatherReadings);
    }
}