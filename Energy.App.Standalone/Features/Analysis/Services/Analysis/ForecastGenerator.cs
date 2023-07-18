using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public class ForecastGenerator : IForecastGenerator
    {
        public List<BasicReading> GetBasicReadingsForecast(decimal degreeDifference,
            Coefficients coefficients,
            List<DailyWeatherRecord> forecastWeatherReadings)
        {
            var adjustedReadings = forecastWeatherReadings
                .SelectMany(c => {
                    decimal adjustedKWh = coefficients.PredictConsumptionY(c.TemperatureAverage + degreeDifference);
                    adjustedKWh = adjustedKWh < 0 ? 0 : adjustedKWh;
                    return Enumerable.Range(0, 48).Select(i => new BasicReading
                        {
                            Forecast = true,
                            UtcTime = c.UtcTime.AddTicks(TimeSpan.TicksPerMinute * 30 * i),
                            KWh = adjustedKWh / 48
                        });
                    }).ToList();

            return adjustedReadings;

        }
    }
}
