using Energy.App.Standalone.Data;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;
using System.Collections.Immutable;
using Energy.App.Standalone.Extensions;

namespace Energy.Test
{
    public class CostCalculatorTests
    {
        public class SingleFixedTariffCostCalculatorTestData : TheoryData<decimal, decimal, decimal, decimal, DateTime, DateTime>
        {
            public SingleFixedTariffCostCalculatorTestData()
            {
                // 30th April 2023 to 31st May 2023
                decimal t1TotalKWhForPeriod = 283m;
                decimal t1PeriodStandingChargePence = 1569m;
                decimal t1PeriodPencePerKWh = 31.697m;
                decimal t1TotalCostForPeriodPence = 10539.251m;

                Add(t1TotalKWhForPeriod,
                    t1PeriodStandingChargePence,
                    t1PeriodPencePerKWh,
                    t1TotalCostForPeriodPence,
                    new DateTime(2023, 4, 30),
                    new DateTime(2023, 5, 31));

                // 31st March 2023 to 30th April 2023
                decimal t2TotalKWhForPeriod = 283m;
                decimal t2PeriodStandingChargePence = 1569m;
                decimal t2PeriodPencePerKWh = 31.697m;
                decimal t2TotalCostForPeriodPence = 10539.251m;


                Add(t2TotalKWhForPeriod,
                    t2PeriodStandingChargePence,
                    t2PeriodPencePerKWh,
                    t2TotalCostForPeriodPence,
                    new DateTime(2023, 3, 31),
                    new DateTime(2023, 4, 30));

            }
        }

        // a test to check that the cost calculator is working correctly

        [Theory]
        [ClassData(typeof(SingleFixedTariffCostCalculatorTestData))]
        public void CostedValuesForSingleApplicableFixedTariff(decimal totalKWhForPeriod,
                                                               decimal periodStandingChargePence,
                                                               decimal periodPencePerKWh,
                                                               decimal totalCostForPeriodPence,
                                                               DateTime start,
                                                               DateTime end)
        {
            int periodDays = start.eGetDateCount(end);

            var myRecentTariff = new TariffDetailState
            {
                DailyStandingChargePence = periodStandingChargePence / periodDays,
                DateAppliesFrom = start,
                PencePerKWh = periodPencePerKWh,
                IsHourOfDayFixed = true,
                GlobalId = Guid.NewGuid(),
                HourOfDayPrices = new()
            };

            var tariffState = myRecentTariff.eItemToImmutableList();

            var basicReadings = CreateBasicReadingsFromTotalKWh(start, end, totalKWhForPeriod).ToImmutableList();

            var costCalculator = new CostCalculator();
            var costedReadings = costCalculator.GetCostReadings(basicReadings, tariffState);
            AssertAccurateForSingleTariff(myRecentTariff,
                                          totalKWhForPeriod,
                                          periodDays,
                                          periodStandingChargePence,
                                          totalCostForPeriodPence,
                                          basicReadings.Count,
                                          costedReadings);
        }

        private static void AssertAccurateForSingleTariff(TariffDetailState applicableTariff,
                                                          decimal totalKWhForPeriod,
                                                          int periodDays,
                                                          decimal periodStandingChargePence,
                                                          decimal totalCostForPeriodPence,
                                                          int numExpectedReadings,
                                                          ImmutableList<CostedReading> calculatedCostedReadings)
        {
            var calculatedTotalKWh = calculatedCostedReadings.Sum(c => c.KWh);
            var calculatedTotalCostPence = calculatedCostedReadings.Sum(c => c.ReadingTotalCostPence);

            var calculatedTotalStandingChargePence = calculatedCostedReadings.Sum(c => c.TariffHalfHourlyStandingChargePence);



            Assert.Equal(numExpectedReadings, calculatedCostedReadings.Count);

            Assert.All<CostedReading>(calculatedCostedReadings, (costReading) =>
            {
                Assert.Equal(applicableTariff.DateAppliesFrom, costReading.TariffAppliesFrom);
                Assert.Equal(applicableTariff.DailyStandingChargePence, costReading.TariffDailyStandingChargePence);
                Assert.True(applicableTariff.HourOfDayPrices.All(c => c.PencePerKWh / 2m == costReading.TariffHalfHourlyPencePerKWh));

                Assert.Equal(totalKWhForPeriod / (periodDays * 48), costReading.KWh);

                Assert.False(costReading.Forecast);

            });

            Assert.Equal(totalKWhForPeriod, calculatedTotalKWh, 6);
            Assert.Equal(periodStandingChargePence, calculatedTotalStandingChargePence, 6);
            Assert.Equal(totalCostForPeriodPence, calculatedTotalCostPence, 6);
        }

        private IEnumerable<BasicReading> CreateBasicReadingsFromTotalKWh(DateTime start, DateTime end, decimal totalKWh)
        {
            var periodDays = start.eGetDateCount(end);

            var totalHalfHours = periodDays * 48;
            decimal kWhPerHalfHour = totalKWh / totalHalfHours;
            return Enumerable.Range(0, totalHalfHours).Select(halfHourIndex =>
            {

                var utcTime = start.AddMinutes(halfHourIndex * 30);

                return new BasicReading()
                {
                    UtcTime = utcTime,
                    KWh = kWhPerHalfHour
                };
            });
        }

        private IEnumerable<HourOfDayPriceState> CreateFixedHourOfDayPrices(decimal v)
        {
            return Enumerable.Range(0, 24).Select(c => new HourOfDayPriceState()
            {
                HourOfDay = TimeSpan.FromHours(c),
                PencePerKWh = v / 24m
            });
        }

    }
}