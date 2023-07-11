using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using System.Collections.Immutable;
using Energy.App.Standalone.Features.Setup.Store.MeterSetupStore.StateObjects;

namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;

public interface ICostCalculator
{
    List<CostedReading> GetCostReadings(IReadOnlyCollection<BasicReading> basicReadings, IEnumerable<TariffDetailState> meterTariffDetails);
}