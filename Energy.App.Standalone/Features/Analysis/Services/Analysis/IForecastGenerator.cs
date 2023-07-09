using Energy.App.Standalone.Features.Analysis.Store;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public interface IForecastGenerator
    {
        List<BasicReading> GetBasicReadingsForecast(decimal degreeDifference, LinearCoefficientsState linearCoefficientsState, List<DailyWeatherReading> forecastWeatherReadings);
    }
}