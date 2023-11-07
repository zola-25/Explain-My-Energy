using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.App.Standalone.Services.FluxorPersist.Demo.JsonModels;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Services.FluxorPersist.Demo.Interfaces;
public interface IDemoReadingsUpdater
{
    UpdatedDemoReadings GetUpdatedReadings(DateTime utcNow, ImmutableList<BasicReading> jsonDemoReadings, ImmutableList<CostedReading> jsonDemoCostedReadings, IEnumerable<TariffDetailState> meterTariffs);
}