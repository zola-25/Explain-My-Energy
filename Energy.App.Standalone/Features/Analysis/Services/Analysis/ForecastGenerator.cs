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
            var adjustedKWh = dailyWeatherReadings.Where(c => c.UtcReadDate >= start && c.UtcReadDate <= end)
                .Select(c => new BasicReading
                {
                    Forecast = true,
                    UtcTime = c.UtcReadDate,
                    KWh = linearCoefficientsState.PredictConsumptionY(c.TemperatureAverage + degreeDifference)
                }).ToImmutableList();

            return adjustedKWh;

        }
    }
}
