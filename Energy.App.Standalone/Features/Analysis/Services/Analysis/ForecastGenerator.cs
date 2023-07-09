using Energy.App.Standalone.Features.Analysis.Store;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public class ForecastGenerator : IForecastGenerator
    {
        public List<BasicReading> GetBasicReadingsForecast(decimal degreeDifference,
            LinearCoefficientsState linearCoefficientsState,
            List<DailyWeatherReading> forecastWeatherReadings)
        {
            var adjustedReadings = forecastWeatherReadings
                .SelectMany(c => {
                    decimal adjustedKWh = linearCoefficientsState.PredictConsumptionY(c.TemperatureAverage + degreeDifference);
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
