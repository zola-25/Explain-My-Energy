using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.Shared;
using MathNet.Numerics.Statistics;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public class ForecastReadingsMovingAverage : IForecastReadingsMovingAverage
    {
        private readonly ICostCalculator _costCalculator;
        private readonly ICostedReadingsToDailyAggregator _costedReadingsToDailyAggregator;

        public ForecastReadingsMovingAverage(ICostCalculator costCalculator, ICostedReadingsToDailyAggregator costedReadingsToDailyAggregator)
        {
            _costCalculator = costCalculator;
            _costedReadingsToDailyAggregator = costedReadingsToDailyAggregator;
        }

        public ImmutableList<DailyCostedReading> GetDailyCostedReadings(
                       ImmutableList<BasicReading> historicalReadings,
                                  ImmutableList<TariffDetailState> meterTariffs)
        {
            var costedReadings = GetCostedReadings(historicalReadings, meterTariffs);
            var dailyCostedReadings = _costedReadingsToDailyAggregator.Aggregate(costedReadings);
            return dailyCostedReadings.ToImmutableList();
        }

        private List<CostedReading> GetCostedReadings(
                ImmutableList<BasicReading> historicalReadings,
                ImmutableList<TariffDetailState> meterTariffs)
        {

            var predictionStartDate = DateTime.UtcNow.Date.AddMonths(-2);
            var predictionEndDate = DateTime.UtcNow.Date.AddMonths(6);

            var predictionDates = predictionStartDate.eGenerateAllDatesBetween(predictionEndDate, true);

            var movingAverages = GetMovingAverageHistoricalReadings(historicalReadings);
                
            var forecastReadings = (
                                   from pred in predictionDates 
                                   join hist in movingAverages 
                                        on pred.DayOfYear equals hist.UtcTime.DayOfYear
                                    select new BasicReading
                                      {
                                        Forecast = true,
                                        KWh = hist.KWh,
                                        UtcTime = pred
                                      }).ToList();
                


            var costedReadings = _costCalculator
                .GetCostReadings(forecastReadings, meterTariffs);
            return costedReadings;
        }

        private IEnumerable<BasicReading> GetMovingAverageHistoricalReadings(ImmutableList<BasicReading> historicalReadings)
        {
            var latestReadingDate = historicalReadings.Last().UtcTime.Date;

            var startHistoricalReadingsDate = latestReadingDate.AddTicks(TimeSpan.TicksPerDay * (-365 - 30)).Date;
            var endHistoricalReadingDate = latestReadingDate.AddTicks(TimeSpan.TicksPerDay * (- 30)).Date;

            var applicableHistoricalReadings = historicalReadings
                .SkipWhile(c=> c.UtcTime.Hour != 0 && c.UtcTime.Minute != 0)
                .Where(x => x.UtcTime >= startHistoricalReadingsDate && x.UtcTime <= endHistoricalReadingDate)
                .ToArray();
                
            var startMovingStatsDate = latestReadingDate.AddTicks(TimeSpan.TicksPerDay * -365).Date;
            var firstWindow = applicableHistoricalReadings.Take(48 * 30).Select(c=>(double)c.KWh).ToList();
            var movingStats = new MovingStatistics(48 * 30, firstWindow);


            for (int i = (48 * 30); i < applicableHistoricalReadings.Length; i++)
            {
                
                var historicalReading = applicableHistoricalReadings[i];
                var movingAverage = movingStats.Mean;

                yield return new BasicReading
                {
                    KWh = (decimal)movingAverage,
                    UtcTime = historicalReading.UtcTime,
                    Forecast = true
                };
                movingStats.Push((double)historicalReading.KWh);


            }
        }
    }
}
