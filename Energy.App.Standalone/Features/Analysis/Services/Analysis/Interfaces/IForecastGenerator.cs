using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces
{
    public interface IForecastGenerator
    {
        List<BasicReading> GetBasicReadingsForecast(decimal degreeDifference, Coefficients coefficients, List<DailyWeatherRecord> forecastWeatherReadings);
    }
}