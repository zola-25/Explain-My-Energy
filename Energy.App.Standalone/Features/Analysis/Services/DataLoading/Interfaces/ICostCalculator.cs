using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;

public interface ICostCalculator
{
    ImmutableList<CostedReading> GetCostReadings(ImmutableList<BasicReading> basicReadings, ImmutableList<TariffDetailState> meterTariffDetails);
}