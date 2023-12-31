﻿using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis;

public class CostedReadingsToDailyAggregator : ICostedReadingsToDailyAggregator
{
    public IEnumerable<DailyCostedReading> Aggregate(IEnumerable<CostedReading> costedReadings)
    {
        var result = costedReadings.GroupBy(c => c.UtcTime.Date).
            Select(c => new DailyCostedReading()
            {
                UtcTime = c.Key,
                ReadingTotalCostPounds = c.Sum(d => d.CostPounds),
                TariffAppliesFrom = c.First().TariffAppliesFrom,
                TariffDailyStandingChargePence = c.First().TariffDailyStandingCharge,
                PencePerKWh = c.Average(d => d.TariffPencePerKWh),
                IsFixedCostPerHour = c.First().IsFixedCostPerHour,
                KWh = c.Sum(d => d.KWh),
            }
            );

        return result;
    }
}
