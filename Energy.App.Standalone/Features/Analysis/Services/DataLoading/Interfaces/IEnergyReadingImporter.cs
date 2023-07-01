using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;

public interface IEnergyReadingImporter
{
    Task<List<BasicReading>> ImportFromMoveIn(Meter meter, CancellationToken ctx = default);
    Task<List<BasicReading>> ImportFromDate(Meter meter, DateTime fromDate, CancellationToken ctx = default);
}