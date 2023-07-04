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
        public class CostCalculatorTestData : TheoryData<decimal, DateTime, DateTime>
        {
            public CostCalculatorTestData()
            {
                Add(283, new DateTime(2023, 4, 30), new DateTime(2023, 5, 31));
            }
        }

        // a test to check that the cost calculator is working correctly

        [Theory]
        [ClassData(typeof(CostCalculatorTestData))]
        public void CostedValuesCorrectForTimePeriod(decimal totalKWhForPeriod, DateTime start, DateTime end)
        {
            int periodDays = start.eGetDateCount(end);;
            decimal periodStandingChargePence = 1569m;
            decimal periodPencePerKWh = 31.697m;
            decimal totalCostForPeriodPence = 10539.251m;

            var myRecentTariff = new TariffDetailState
            {
                DailyStandingChargePence = periodStandingChargePence / periodDays,
                DateAppliesFrom = new DateTime(2023, 4, 30),
                PencePerKWh = periodPencePerKWh,
                IsHourOfDayFixed = true,
                GlobalId = Guid.NewGuid(),
                HourOfDayPrices = new()
            };

            var tarriffDetails = DefaultTariffData.DefaultTariffs.Select(c => new TariffDetailState
            {
                DailyStandingChargePence = c.DailyStandingChargePence,
                PencePerKWh = c.PencePerKWh,
                DateAppliesFrom = c.DateAppliesFrom,
                IsHourOfDayFixed = c.IsHourOfDayFixed,
                HourOfDayPrices = c.DefaultHourOfDayPrices.eMapToHourOfDayPriceState(),
                GlobalId = Guid.NewGuid(),
            }).Append(myRecentTariff).ToImmutableList();

            var basicReadings = CreateBasicReadingsFromTotalKWh(start, end, totalKWhForPeriod).ToImmutableList();
            
            var costCalculator = new CostCalculator();
            var costedReadings = costCalculator.GetCostReadings(basicReadings, tarriffDetails);
            
            var calculatedTotalKWh = costedReadings.Sum(c => c.KWh);
            var calculatedTotalCostPence = costedReadings.Sum(c => c.ReadingTotalCostPence);

            var calculatedTotalStandingChargePence = costedReadings.Sum(c => c.TariffHalfHourlyStandingChargePence);


            var applicableTariff = tarriffDetails.Where(c => c.DateAppliesFrom <= start).OrderByDescending(c => c.DateAppliesFrom).First();

            Assert.Equal(basicReadings.Count, costedReadings.Count);

            Assert.All<CostedReading>(costedReadings, (costReading) =>
            {
                Assert.Equal(applicableTariff.DateAppliesFrom, costReading.TariffAppliesFrom);
                Assert.Equal(applicableTariff.DailyStandingChargePence, costReading.TariffDailyStandingChargePence);
                Assert.True(applicableTariff.HourOfDayPrices.All(c => c.PencePerKWh  / 2m == costReading.TariffHalfHourlyPencePerKWh));

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
            return Enumerable.Range(0, totalHalfHours).Select(halfHourIndex => {
                
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