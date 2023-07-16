using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Validation
{
    public class HistoricalForecastValidation : IHistoricalForecastValidation
    {
        public bool Validate(MeterState meterSetup,
                bool forceRefresh,
                DateTime lastUpdate,
                ImmutableList<BasicReading> existingHistoricalReadings,
                ImmutableList<DailyCostedReading> existingForecastReadings,
                TaskCompletionSource<(bool, string)> taskCompletionSource)
        {
            if (meterSetup == null
                || !meterSetup.SetupValid)
            {
                taskCompletionSource?.SetResult((false, $"{meterSetup.MeterType} meter not setup"));
                return false;
            }

            if (meterSetup.TariffDetails.eIsNullOrEmpty())
            {
                taskCompletionSource?.SetResult((false, $"{meterSetup.MeterType} meter tariffs not setup"));
                return false;
            }

            if (existingHistoricalReadings.eIsNullOrEmpty())
            {
                taskCompletionSource?.SetResult((false, $"No historical {meterSetup.MeterType} readings"));
                return false;
            }

            if (existingHistoricalReadings.First().UtcTime > DateTime.UtcNow.Date.AddDays(-180))
            {
                taskCompletionSource?.SetResult((false, $"Not enough historical {meterSetup.MeterType} readings"));
                return false;
            }

            if (!forceRefresh
                && existingForecastReadings.eIsNotNullOrEmpty()
                && existingForecastReadings.Count > 180
                && lastUpdate > DateTime.UtcNow.Date.AddDays(-7))
            {
                taskCompletionSource?.SetResult((true, "Forecast already exists"));
                return false;
            }

            return true;

        }
    }
}
