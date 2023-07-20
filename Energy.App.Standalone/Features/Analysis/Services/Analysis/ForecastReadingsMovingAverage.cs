using Energy.App.Standalone.Data;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
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

        public ImmutableList<DailyCostedReading> GetDailyCostedReadings(ImmutableList<BasicReading> historicalReadings,
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

            var predictionDates = AppWideForecastProperties.PredictionDates(historicalReadings.Last().UtcTime);

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
            var latestReadingDate = historicalReadings.Last().UtcTime;

            var dayWindowSize = AppWideForecastProperties.MovingAverageWindowSizeDays;
            var hhWindowSize = AppWideForecastProperties.MovingAverageWindowSizeHalfHours;

            var minMovingAverageDate = latestReadingDate.AddYears(-1).AddTicks(TimeSpan.TicksPerDay * (-dayWindowSize)).Date;
            var maxMovingAverageDate = latestReadingDate.AddTicks(TimeSpan.TicksPerDay * (-dayWindowSize)).Date;

            var movingAverageInputReadings = historicalReadings
                .SkipWhile(c => c.UtcTime < minMovingAverageDate)
                .Where(x => x.UtcTime < maxMovingAverageDate)
                .ToArray();

            var firstWindowReadings = movingAverageInputReadings[..hhWindowSize].Select(c => (double)c.KWh);
            var movingStats = new MovingStatistics(hhWindowSize, firstWindowReadings);
            var movingAverageIterateArray = movingAverageInputReadings[hhWindowSize..];

            for (int i = 0; i < movingAverageIterateArray.Length; i++)
            {
                var historicalReading = movingAverageIterateArray[i];
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
