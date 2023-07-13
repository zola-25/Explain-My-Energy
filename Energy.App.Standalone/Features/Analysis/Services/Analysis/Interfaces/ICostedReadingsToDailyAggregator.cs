using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces
{
    public interface ICostedReadingsToDailyAggregator
    {
        IEnumerable<DailyCostedReading> Aggregate(IEnumerable<CostedReading> costedReadings);
    }
}