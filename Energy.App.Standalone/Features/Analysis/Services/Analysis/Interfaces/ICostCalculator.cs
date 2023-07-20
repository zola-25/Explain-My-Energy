using Energy.Shared;
using System.Collections.Immutable;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

public interface ICostCalculator
{
    List<CostedReading> GetCostReadings(IReadOnlyCollection<BasicReading> basicReadings, IEnumerable<TariffDetailState> meterTariffDetails);
}