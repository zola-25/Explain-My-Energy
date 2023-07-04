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
            var tarriffDetails = DefaultTariffData.DefaultTariffs.Select(c => new TariffDetailState
            {
                DailyStandingChargePence = c.DailyStandingChargePence,
                PencePerKWh = c.PencePerKWh,
                DateAppliesFrom = c.DateAppliesFrom,
                IsHourOfDayFixed = c.IsHourOfDayFixed,
                HourOfDayPrices = c.DefaultHourOfDayPrices.eMapToHourOfDayPriceState(),
                GlobalId = Guid.NewGuid(),

            }).ToImmutableList();

            var costCalculator = new CostCalculator();
            var basicReadings = CreateBasicReadingsFromTotalKWh(start, end, totalKWhForPeriod).ToImmutableList();
            
            var costReadings = costCalculator.GetCostReadings(basicReadings, tarriffDetails);
            var totalKWh = costReadings.Sum(c => c.KWh);
            var totalCost = costReadings.Sum(c => c.ReadingTotalCostPence);

            var totalStandingCharge = costReadings.Sum(c => c.TariffHalfHourlyStandingChargePence);


            var applicableTariff = tarriffDetails.Where(c => c.DateAppliesFrom <= start).OrderByDescending(c => c.DateAppliesFrom).First();

            Assert.All<CostedReading>(costReadings, (costReading) =>
            {
                Assert.Equal(applicableTariff.DateAppliesFrom, costReading.TariffAppliesFrom);
                Assert.Equal(applicableTariff.DailyStandingChargePence, costReading.TariffDailyStandingChargePence);
                Assert.True(applicableTariff.HourOfDayPrices.All(c => c.PencePerKWh  / 2m == costReading.TariffHalfHourlyPencePerKWh));
                
                Assert.Equal(totalKWh / (end - start).Days, costReading.KWh);

                Assert.False(costReading.Forecast);

            });

            Assert.Equal(283, totalKWh);
            Assert.Equal(1569, totalStandingCharge);
            Assert.Equal(10539, totalCost);
        }

        private IEnumerable<BasicReading> CreateBasicReadingsFromTotalKWh(DateTime start, DateTime end, decimal totalKWh)
        {
            // create a list of basic readings from a total kWh divided int 24 hourly readings
            var totalDays = (end - start).Days;
            var totalHalfHours = totalDays * 48;
            decimal kWhPerHalfHour = totalKWh / (decimal)totalHalfHours;
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