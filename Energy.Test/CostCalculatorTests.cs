using Energy.App.Standalone.Data;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;
using System.Collections.Immutable;
using Energy.App.Standalone.Extensions;
using System.Xml.Schema;
using FluentAssertions;
using MathNet.Numerics;

namespace Energy.Test
{
    public class CostCalculatorTests
    {
        public class SingleFixedTariffCostCalculatorTestData : TheoryData<MeterType, decimal, decimal, decimal, DateTime, DateTime>
        {
            public SingleFixedTariffCostCalculatorTestData()
            {
                // Electricity 31st March 2023 to 30th April 2023
                var electricityTestValues = Data.ElectricityPeriodValuesList.First(c => c.BillDate == BillDate.May);

                Add(MeterType.Electricity,
                    electricityTestValues.TotalKWhForPeriod,
                    electricityTestValues.PeriodStandingChargePence,
                    electricityTestValues.TotalCostForPeriodPence,
                    electricityTestValues.Start,
                    electricityTestValues.End);


                // Gas 31st March 2023 to 30th April 2023
                var gasTestValues = Data.GasPeriodValuesList.First(c => c.BillDate == BillDate.May);

                Add(MeterType.Gas,
                    gasTestValues.TotalKWhForPeriod,
                    gasTestValues.PeriodStandingChargePence,
                    gasTestValues.TotalCostForPeriodPence,
                    gasTestValues.Start,
                    gasTestValues.End);

            }
        }

        // a test to check that the cost calculator is working correctly

        [Theory]
        [ClassData(typeof(SingleFixedTariffCostCalculatorTestData))]
        public void CostedValuesForSingleApplicableFixedTariff(MeterType meterType,
                                                                decimal totalKWhForPeriod,
                                                               decimal periodStandingChargePence,
                                                               decimal totalCostForPeriodPence,
                                                               DateTime start,
                                                               DateTime end)
        {
            int periodDays = start.eGetDateCountExcludeEnd(end);
            var tariffState = Data.GetTariffDetailState(start, meterType);

            var basicReadings = CreateBasicReadingsFromTotalKWh(start, end, totalKWhForPeriod).ToImmutableList();

            var costCalculator = new CostCalculator();
            var costedReadings = costCalculator.GetCostReadings(basicReadings, tariffState.eItemToImmutableList());
            AssertAccurateForSingleTariff(tariffState,
                                          totalKWhForPeriod,
                                          periodDays,
                                          periodStandingChargePence,
                                          totalCostForPeriodPence,
                                          basicReadings.Count,
                                          costedReadings);
        }

        [Fact]
        public void CostedValuesForMultipleFixedTariffs()
        {
            var data = Data.ElectricityPeriodValuesList;
            var start = data.First().Start;
            var end = data.Last().End;

            int totalDays = start.eGetDateCountExcludeEnd(end);

            var tariffs = Data.TestElectricityTariffs.ToImmutableList();

            decimal expectedTotalKWh = data.Sum(c => c.TotalKWhForPeriod);
            decimal expectedTotalStandingChargePence = data.Sum(c => c.PeriodStandingChargePence);
            decimal expectedTotalCostPence = data.Sum(c => c.TotalCostForPeriodPence);


            var basicReadings = CreateBasicReadingsFromTotalKWh(start, end, expectedTotalKWh).ToImmutableList();

            var costCalculator = new CostCalculator();
            var calculatedCostedReadings = costCalculator.GetCostReadings(basicReadings, tariffs);

            var calculatedTotalKWh = calculatedCostedReadings.Sum(c => c.KWh);
            var calculatedTotalCostPence = calculatedCostedReadings.Sum(c => c.ReadingTotalCostPence);

            var calculatedTotalStandingChargePence = calculatedCostedReadings.Sum(c => c.TariffHalfHourlyStandingChargePence);



            Assert.Equal(basicReadings.Count, calculatedCostedReadings.Count);

            var expectedReadingProperties = basicReadings.Select(c => new SimpleReadingProperties
            {
                UtcTime = c.UtcTime,
                KWh = c.KWh
            }).ToList();

            var calculatedReadingProperties = calculatedCostedReadings.Select(c => new SimpleReadingProperties
            {
                UtcTime = c.UtcTime,
                KWh = c.KWh
            }).ToList();

            expectedReadingProperties.Should().BeEquivalentTo(
                calculatedReadingProperties,
                c => c.ComparingRecordsByMembers().WithStrictOrdering()
            );

            Assert.All<CostedReading>(calculatedCostedReadings, (costReading) =>
            {
                var applicableTariff = tariffs.First(c => c.DateAppliesFrom == costReading.TariffAppliesFrom);

                Assert.Equal(applicableTariff.DailyStandingChargePence, costReading.TariffDailyStandingChargePence);

                Assert.Equal(applicableTariff.PencePerKWh, costReading.TariffHalfHourlyPencePerKWh);

                Assert.Equal(expectedTotalKWh / (totalDays * 48), costReading.KWh);

                decimal expectedCost = (costReading.KWh *  applicableTariff.PencePerKWh).Round(3)  + (applicableTariff.DailyStandingChargePence / 48).Round(2);
                Assert.Equal(expectedCost, costReading.ReadingTotalCostPence);


                Assert.False(costReading.Forecast);

            });


            Assert.Equal(expectedTotalKWh, calculatedTotalKWh, 0);
            Assert.Equal(expectedTotalStandingChargePence, calculatedTotalStandingChargePence, 0);
            Assert.Equal(expectedTotalCostPence, calculatedTotalCostPence, 0);
        }

        public record SimpleReadingProperties
        {
            public DateTime UtcTime { get; init; }
            public decimal KWh { get; init; }
        }

        //[Theory]
        //[ClassData(typeof(SingleFixedTariffCostCalculatorTestData))]
        //public void CostedValuesForMultipleApplicableFixedTariff(decimal totalKWhForPeriod,
        //                                                       decimal periodStandingChargePence,
        //                                                       decimal periodPencePerKWh,
        //                                                       decimal totalCostForPeriodPence,
        //                                                       DateTime start,
        //                                                       DateTime end)
        //{
        //    int periodDays = start.eGetDateCountExcludeEnd(end);

        //    var myRecentTariff = new TariffDetailState
        //    {
        //        DailyStandingChargePence = periodStandingChargePence / periodDays,
        //        DateAppliesFrom = start,
        //        PencePerKWh = periodPencePerKWh,
        //        IsHourOfDayFixed = true,
        //        GlobalId = Guid.NewGuid(),
        //        HourOfDayPrices = new()
        //    };

        //    var tariffState = myRecentTariff.eItemToImmutableList();

        //    var basicReadings = CreateBasicReadingsFromTotalKWh(start, end, totalKWhForPeriod).ToImmutableList();

        //    var costCalculator = new CostCalculator();
        //    var costedReadings = costCalculator.GetCostReadings(basicReadings, tariffState);
        //    AssertAccurateForSingleTariff(myRecentTariff,
        //                                  totalKWhForPeriod,
        //                                  periodDays,
        //                                  periodStandingChargePence,
        //                                  totalCostForPeriodPence,
        //                                  basicReadings.Count,
        //                                  costedReadings);
        //}

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

                Assert.True(costReading.TariffHalfHourlyPencePerKWh == applicableTariff.PencePerKWh);

                Assert.Equal(totalKWhForPeriod / (periodDays * 48), costReading.KWh);

                Assert.False(costReading.Forecast);

            });


            Assert.Equal(totalKWhForPeriod, calculatedTotalKWh, 0);
            Assert.Equal(periodStandingChargePence, calculatedTotalStandingChargePence, 0);
            Assert.Equal(totalCostForPeriodPence, calculatedTotalCostPence, 0);
        }



        private IEnumerable<BasicReading> CreateBasicReadingsFromTotalKWh(DateTime start, DateTime end, decimal totalKWh)
        {
            var periodDays = start.eGetDateCountExcludeEnd(end);

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