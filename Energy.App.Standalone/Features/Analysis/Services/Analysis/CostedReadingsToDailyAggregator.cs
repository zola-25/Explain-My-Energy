using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public class CostedReadingsToDailyAggregator : ICostedReadingsToDailyAggregator
    {
        public IEnumerable<DailyCostedReading> Aggregate(IEnumerable<CostedReading> costedReadings)
        {
            var result = costedReadings.GroupBy(c => c.UtcTime.Date).
                Select
                (
                    c => new DailyCostedReading()
                    {
                        UtcTime = c.Key,
                        ReadingTotalCostPounds = c.Sum(d => d.CostPounds),
                        TariffAppliesFrom = c.First().
                            TarrifAppliesFrom,
                        TariffDailyStandingChargePence = c.First().
                            TariffDailyStandingCharge,
                        PencePerKWh = c.Average(d => d.TariffPencePerKWh),
                        IsFixedCostPerHour = c.First().
                            IsFixedCostPerHour,
                        KWh = c.Sum(d => d.KWh),
                        Forecast = c.First().IsForecast
                    }
                );

            return result;
        }
    }
}
