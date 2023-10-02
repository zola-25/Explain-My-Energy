using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

public interface IForecastReadingsMovingAverage
{
    ImmutableList<DailyCostedReading> GetDailyCostedReadings(ImmutableList<BasicReading> historicalReadings, ImmutableList<TariffDetailState> meterTariffs);
}