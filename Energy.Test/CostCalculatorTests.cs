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
                var electricityTestValues = Data.ElectricityBillValuesList.First(c => c.BillDate == BillDate.May);

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

            var basicReadings = Data.CreateBasicReadingsFromTotalKWh(start, end, totalKWhForPeriod).ToImmutableList();

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
        public void CostedValuesForRandomTariffsThatDontOverlapWithBasicReadings()
        {
            
            var tariffs = Data.EnergyTariffsManuallyCreated;
            var energyData = Data.EnergyBillValuesManuallyCreated;

            var start = energyData.First().Start;
            var end = energyData.Last().End;
            var basicReadings = Data.GetBasicReadingsManuallyCreated(start, end);

            decimal expectedTotalKWh = energyData.Sum(c => c.TotalKWhForPeriod);
            decimal expectedTotalCost = energyData.Sum(c => c.TotalCostForPeriodPence);
            decimal expectedTotalStandingCharge = energyData.Sum(c => c.PeriodStandingChargePence);

            var costCalculator = new CostCalculator();
            var calculatedCostedReadings = costCalculator.GetCostReadings(basicReadings, tariffs);

            Assert.Equal(basicReadings.Count, calculatedCostedReadings.Count);

            basicReadings.Select(c => new SimpleReadingProperties { KWh = c.KWh, UtcTime = c.UtcTime })
            .Should()
            .BeEquivalentTo(
                calculatedCostedReadings.Select(c => new SimpleReadingProperties { KWh = c.KWh, UtcTime = c.UtcTime }),
                e => e.ComparingRecordsByMembers().WithStrictOrdering());


            var halfHourlyPriceTariffLookup = tariffs.Single(c => !c.IsHourOfDayFixed).HourOfDayPrices.SelectMany(ToHalfHourly).ToDictionary(c => c.HourOfDay.Value);

            Assert.All<CostedReading>(calculatedCostedReadings, (costReading) =>
            {
                var applicableTariff = tariffs.First(c => c.DateAppliesFrom == costReading.TariffAppliesFrom);
                Assert.Equal(applicableTariff.DailyStandingChargePence, costReading.TariffDailyStandingChargePence);

                if (applicableTariff.IsHourOfDayFixed)
                {
                    Assert.Equal(applicableTariff.PencePerKWh, costReading.TariffHalfHourlyPencePerKWh);

                    decimal expectedCost = (costReading.KWh * applicableTariff.PencePerKWh) + (applicableTariff.DailyStandingChargePence / 48);
                    Assert.Equal(expectedCost, costReading.ReadingTotalCostPence);

                }
                else
                {
                    var halfHourlyPrice = halfHourlyPriceTariffLookup[costReading.UtcTime.TimeOfDay];

                    Assert.Equal(halfHourlyPrice.PencePerKWh, costReading.TariffHalfHourlyPencePerKWh);

                    decimal expectedCost = (costReading.KWh * halfHourlyPrice.PencePerKWh) + (applicableTariff.DailyStandingChargePence / 48);
                    Assert.Equal(expectedCost, costReading.ReadingTotalCostPence);
                }

                Assert.False(costReading.Forecast);

            });

            decimal calculatedTotalKWh = calculatedCostedReadings.Sum(c => c.KWh);
            decimal calculatedTotalCost = calculatedCostedReadings.Sum(c => c.ReadingTotalCostPence);
            decimal calculatedTotalStandingCharge = calculatedCostedReadings.Sum(c => c.TariffHalfHourlyStandingChargePence);

            Assert.Equal(expectedTotalKWh, calculatedTotalKWh, 0);
            Assert.Equal(expectedTotalStandingCharge, calculatedTotalStandingCharge, 0);

            Assert.Equal(expectedTotalCost, calculatedTotalCost, 0);
            
        }

        private static IEnumerable<HourOfDayPriceState> ToHalfHourly(HourOfDayPriceState hourOfDayPrice)
        {

            yield return
                new HourOfDayPriceState
                {
                    HourOfDay = new TimeSpan (hourOfDayPrice.HourOfDay.Value.Hours,0,0),
                    PencePerKWh = hourOfDayPrice.PencePerKWh,
                };

            yield return new HourOfDayPriceState
            {
                HourOfDay = new TimeSpan (hourOfDayPrice.HourOfDay.Value.Hours,30,0),
                PencePerKWh = hourOfDayPrice.PencePerKWh,
            };
        }


        [Fact]
        public void CostedValuesForMultipleFixedTariffs()
        {
            var energyBillValues = Data.ElectricityBillValuesList;
            var start = energyBillValues.First().Start;
            var end = energyBillValues.Last().End;

            int totalDays = start.eGetDateCountExcludeEnd(end);

            var tariffs = Data.TestElectricityTariffs.ToImmutableList();

            decimal expectedTotalKWh = energyBillValues.Sum(c => c.TotalKWhForPeriod);
            decimal expectedTotalStandingChargePence = energyBillValues.Sum(c => c.PeriodStandingChargePence);
            decimal expectedTotalCostPence = energyBillValues.Sum(c => c.TotalCostForPeriodPence);


            var basicReadings = Data.CreateBasicReadingsBillValues(energyBillValues).ToImmutableList();

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

                decimal expectedCost = (costReading.KWh * applicableTariff.PencePerKWh) + (applicableTariff.DailyStandingChargePence / 48);
                Assert.Equal(expectedCost, costReading.ReadingTotalCostPence);


                Assert.False(costReading.Forecast);

            });


            Assert.Equal(expectedTotalKWh, calculatedTotalKWh, 0);
            Assert.Equal(expectedTotalStandingChargePence, calculatedTotalStandingChargePence, 0);

            // each bill total cost is provided rounded rather than exact, hence sum of all totals is out by 1p
            expectedTotalCostPence.Should().BeApproximately(calculatedTotalCostPence, 1m);

        }

        public record SimpleReadingProperties
        {
            public DateTime UtcTime { get; init; }
            public decimal KWh { get; init; }
        }


        private static void AssertAccurateForSingleTariff(TariffDetailState applicableTariff,
                                                          decimal totalKWhForPeriod,
                                                          int periodDays,
                                                          decimal periodStandingChargePence,
                                                          decimal totalCostForPeriodPence,
                                                          int numExpectedReadings,
                                                          List<CostedReading> calculatedCostedReadings)
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
    }
}