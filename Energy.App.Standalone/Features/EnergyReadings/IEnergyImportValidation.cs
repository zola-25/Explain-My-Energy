using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.EnergyReadings
{
    public interface IEnergyImportValidation
    {
        EnergyImportValidationResult Validate(MeterState meterSetup, bool forceReload, DateTime lastReadingsCheck, ImmutableList<BasicReading> existingBasicReadings, ImmutableList<CostedReading> existingCostedReadings);
    }
}