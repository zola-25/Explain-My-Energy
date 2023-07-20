using Energy.App.Standalone.Features.Analysis.Services.DataLoading;
using Energy.Shared;
using System.Collections.Immutable;
using Energy.App.Standalone.Extensions;
using FluentAssertions;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

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
                    electricityTestValues.TotalCostForPeriodPence / 100,
                    electricityTestValues.Start,
                    electricityTestValues.End);


                // Gas 31st March 2023 to 30th April 2023
                var gasTestValues = Data.GasPeriodValuesList.First(c => c.BillDate == BillDate.May);

                Add(MeterType.Gas,
                    gasTestValues.TotalKWhForPeriod,
                    gasTestValues.PeriodStandingChargePence,
                    gasTestValues.TotalCostForPeriodPence / 100,
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
                                                               decimal totalCostForPeriodPounds,
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
                                          totalCostForPeriodPounds,
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
            decimal expectedTotalCostPounds = energyData.Sum(c => c.TotalCostForPeriodPence) / 100;
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
                var applicableTariff = tariffs.First(c => c.DateAppliesFrom == costReading.TarrifAppliesFrom);
                Assert.Equal(applicableTariff.DailyStandingChargePence, costReading.TariffDailyStandingCharge);

                if (applicableTariff.IsHourOfDayFixed)
                {
                    Assert.Equal(applicableTariff.PencePerKWh, costReading.TariffPencePerKWh);

                    decimal expectedCost =((costReading.KWh * applicableTariff.PencePerKWh) + (applicableTariff.DailyStandingChargePence / 48)) / 100;
                    Assert.Equal(expectedCost, costReading.CostPounds);

                }
                else
                {
                    var halfHourlyPrice = halfHourlyPriceTariffLookup[costReading.UtcTime.TimeOfDay];

                    Assert.Equal(halfHourlyPrice.PencePerKWh, costReading.TariffPencePerKWh);

                    decimal expectedCost = ((costReading.KWh * halfHourlyPrice.PencePerKWh) + (applicableTariff.DailyStandingChargePence / 48)) / 100;
                    Assert.Equal(expectedCost, costReading.CostPounds);
                }

                Assert.False(costReading.IsForecast);

            });

            decimal calculatedTotalKWh = calculatedCostedReadings.Sum(c => c.KWh);
            decimal calculatedTotalCostPounds = calculatedCostedReadings.Sum(c => c.CostPounds);
            decimal calculatedTotalStandingCharge = calculatedCostedReadings.Sum(c => c.TariffHalfHourlyStandingCharge);

            Assert.Equal(expectedTotalKWh, calculatedTotalKWh, 0);
            Assert.Equal(expectedTotalStandingCharge, calculatedTotalStandingCharge, 0);

            Assert.Equal(expectedTotalCostPounds, calculatedTotalCostPounds, 0);
            
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
            decimal expectedTotalCostPounds = energyBillValues.Sum(c => c.TotalCostForPeriodPence) / 100;


            var basicReadings = Data.CreateBasicReadingsBillValues(energyBillValues).ToImmutableList();

            var costCalculator = new CostCalculator();
            var calculatedCostedReadings = costCalculator.GetCostReadings(basicReadings, tariffs);

            var calculatedTotalKWh = calculatedCostedReadings.Sum(c => c.KWh);
            var calculatedTotalCostPence = calculatedCostedReadings.Sum(c => c.CostPounds);

            var calculatedTotalStandingChargePence = calculatedCostedReadings.Sum(c => c.TariffHalfHourlyStandingCharge);



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
                var applicableTariff = tariffs.First(c => c.DateAppliesFrom == costReading.TarrifAppliesFrom);

                Assert.Equal(applicableTariff.DailyStandingChargePence, costReading.TariffDailyStandingCharge);

                Assert.Equal(applicableTariff.PencePerKWh, costReading.TariffPencePerKWh);

                decimal expectedCost = ((costReading.KWh * applicableTariff.PencePerKWh) + (applicableTariff.DailyStandingChargePence / 48)) / 100;
                Assert.Equal(expectedCost, costReading.CostPounds);


                Assert.False(costReading.IsForecast);

            });


            Assert.Equal(expectedTotalKWh, calculatedTotalKWh, 0);
            Assert.Equal(expectedTotalStandingChargePence, calculatedTotalStandingChargePence, 0);

            // each bill total cost is provided rounded rather than exact, hence sum of all totals is out by 1p
            expectedTotalCostPounds.Should().BeApproximately(calculatedTotalCostPence, 1m);

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
            var calculatedTotalCostPounds = calculatedCostedReadings.Sum(c => c.CostPounds);

            var calculatedTotalStandingChargePence = calculatedCostedReadings.Sum(c => c.TariffHalfHourlyStandingCharge);



            Assert.Equal(numExpectedReadings, calculatedCostedReadings.Count);

            Assert.All<CostedReading>(calculatedCostedReadings, (costReading) =>
            {
                Assert.Equal(applicableTariff.DateAppliesFrom, costReading.TarrifAppliesFrom);
                Assert.Equal(applicableTariff.DailyStandingChargePence, costReading.TariffDailyStandingCharge);

                Assert.True(costReading.TariffPencePerKWh == applicableTariff.PencePerKWh);

                Assert.Equal(totalKWhForPeriod / (periodDays * 48), costReading.KWh);

                Assert.False(costReading.IsForecast);

            });


            Assert.Equal(totalKWhForPeriod, calculatedTotalKWh, 0);
            Assert.Equal(periodStandingChargePence, calculatedTotalStandingChargePence, 0);
            Assert.Equal(totalCostForPeriodPence, calculatedTotalCostPounds, 0);
        }
    }
}