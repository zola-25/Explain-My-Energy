using Energy.App.Standalone.Features.Analysis.Store;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public class ForecastGenerator : IForecastGenerator
    {
        public ImmutableList<BasicReading> GetBasicReadingsForecast(DateTime start,
                                                                   DateTime end,
                                                                   decimal degreeDifference,
                                                                   LinearCoefficientsState linearCoefficientsState,
                                                                   ImmutableList<DailyWeatherReading> dailyWeatherReadings)
        {
            var adjustedReadings = dailyWeatherReadings.Where(c => c.UtcReadDate >= start && c.UtcReadDate <= end)
                .SelectMany(c => {
                    decimal adjustedKWh = linearCoefficientsState.PredictConsumptionY(c.TemperatureAverage + degreeDifference);
                    return Enumerable.Range(0, 48).Select(i => new BasicReading
                        {
                            Forecast = true,
                            UtcTime = c.UtcReadDate.AddTicks(TimeSpan.TicksPerMinute * 30 * i),
                            KWh = adjustedKWh / 48
                        });
                    }).ToImmutableList();

            return adjustedReadings;

        }
    }
}
