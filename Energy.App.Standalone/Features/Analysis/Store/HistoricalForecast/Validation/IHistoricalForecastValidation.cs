using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Validation
{
    public interface IHistoricalForecastValidation
    {
        bool Validate(MeterState meterSetup,
                      bool forceRefresh,
                      DateTime lastUpdate,
                      ImmutableList<BasicReading> existingHistoricalReadings,
                      ImmutableList<DailyCostedReading> existingForecastReadings,
                      TaskCompletionSource<(bool, string)> taskCompletionSource);
    }
}