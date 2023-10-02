using Energy.App.Standalone.Data;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Validation;

public class HistoricalForecastValidationResult
{
    public bool CanUpdate { get; init; }
    public string Message { get; init; }
    public bool IsWarning { get; init; }
}
public class HistoricalForecastValidation : IHistoricalForecastValidation
{
    public HistoricalForecastValidationResult Validate(MeterState meterSetup,
                                                       bool forceRefresh,
                                                       DateTime lastUpdate,
                                                       ImmutableList<BasicReading> existingHistoricalReadings,
                                                       ImmutableList<DailyCostedReading> existingForecastReadings)
    {
        var meterType = meterSetup.MeterType;
        if (meterSetup == null
            || !meterSetup.SetupValid)
        {
            return new HistoricalForecastValidationResult
            {
                CanUpdate = false,
                IsWarning = true,
                Message = $"{meterType} meter not setup"
            };
        }

        if (meterSetup.TariffDetails.eIsNullOrEmpty())
        {
            return new HistoricalForecastValidationResult
            {
                CanUpdate = false,
                IsWarning = true,
                Message = $"{meterType} meter tariffs not setup"
            };
        }

        if (existingHistoricalReadings.eIsNullOrEmpty())
        {
            return new HistoricalForecastValidationResult
            {
                CanUpdate = false,
                IsWarning = true,
                Message = $"No historical {meterType} readings"
            };
        }
        var latestReadingDate = existingHistoricalReadings.Last().UtcTime;
        var earliestReadingDate = existingHistoricalReadings.First().UtcTime;

        var numDays = (latestReadingDate - earliestReadingDate).TotalDays;
        if (numDays < 180)
        {
            return new HistoricalForecastValidationResult
            {
                CanUpdate = false,
                IsWarning = true,
                Message = $"Not enough historical {meterType} readings - need data {180} days ago, earliest historical reading is {earliestReadingDate.eDateToDowShortMonthYY()}"
            };
        }

        if (!forceRefresh
            && existingForecastReadings.eIsNotNullOrEmpty()
            && existingForecastReadings.Count >= 180
            && lastUpdate > DateTime.UtcNow.Date.AddDays(-7))
        {
            return new HistoricalForecastValidationResult
            {
                CanUpdate = false,
                IsWarning = false,
                Message = $"{meterType} Historical Forecast already exists"
            };
        }

        return new HistoricalForecastValidationResult
        {
            CanUpdate = true,
            IsWarning = false,
            Message = "Can update"
        };

    }
}
