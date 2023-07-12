using Energy.App.Standalone.Extensions;
using Energy.Shared;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;

namespace Energy.Test
{
    public static class Data
    {
        // Function to get tariff details for a given Date and meter type
        public static TariffDetailState GetTariffDetailState(DateTime date, MeterType meterType)
        {
            var tariffDetailState = (meterType switch
            {
                MeterType.Gas => TestGasTariffs,
                MeterType.Electricity => TestElectricityTariffs,
                _ => throw new ArgumentOutOfRangeException(nameof(meterType), meterType, null)
            }).FirstOrDefault(c => c.DateAppliesFrom == date);

            if (tariffDetailState == null)
            {
                throw new Exception($"No tariff found for {meterType} on {date}");
            }

            return tariffDetailState;
        }

        public static ImmutableList<BasicReading> GetBasicReadingsManuallyCreated(DateTime start, DateTime end)
        {
            var basicReadings = new List<BasicReading>();
            int days = start.eGetDateCountExcludeEnd(end);

            var halfHours = days * 48;

            // if the hour is divisible by 6, then the price is 0.08
            // if the hour is divisible by 2, then the price is 0.02
            // if the hour is divisible by 3, then the price is 0.04
            // otherwise the price is 0.07


            for (int i = 0; i < halfHours; i++)
            {
                var readTime = start.AddTicks(TimeSpan.TicksPerMinute * 30 * i);
                var basicReading = new BasicReading
                {
                    UtcTime = readTime,
                    KWh =
                    readTime.Hour % 6 == 0 ? 0.08m :
                    readTime.Hour % 2 == 0 ? 0.02m :
                    readTime.Hour % 3 == 0 ? 0.04m :
                    0.07m
                };
                basicReadings.Add(basicReading);
            }

            return basicReadings.ToImmutableList();
        }


        public static IEnumerable<BasicReading> CreateBasicReadingsFromTotalKWh(DateTime start, DateTime end, decimal totalKWh)
        {
            int periodDays = start.eGetDateCountExcludeEnd(end);

            int totalHalfHours = periodDays * 48;
            decimal kWhPerHalfHour = totalKWh / totalHalfHours;
            return Enumerable.Range(0, totalHalfHours).Select(halfHourIndex =>
            {

                var utcTime = start.AddTicks(TimeSpan.TicksPerMinute * 30 * halfHourIndex);

                return new BasicReading()
                {
                    UtcTime = utcTime,
                    KWh = kWhPerHalfHour
                };
            });
        }


        public static IEnumerable<BasicReading> CreateBasicReadingsBillValues(ImmutableList<EnergyBillValues> periodValues)
        {
            return periodValues.Select(c => Data.CreateBasicReadingsFromTotalKWh(c.Start, c.End, c.TotalKWhForPeriod)).SelectMany(c => c).ToList();

        }



        public static List<TariffDetailState> TestElectricityTariffs => ElectricityBillValuesList.Select(c =>
        {
            int periodDays = c.Start.eGetDateCountExcludeEnd(c.End);

            return new TariffDetailState
            {
                DateAppliesFrom = c.Start,
                PencePerKWh = c.PeriodPencePerKWh,
                DailyStandingChargePence = (c.PeriodStandingChargePence / periodDays),
                IsHourOfDayFixed = true,
                HourOfDayPrices = ImmutableList<HourOfDayPriceState>.Empty
            };
        }).ToList();

        public static List<TariffDetailState> TestGasTariffs => GasPeriodValuesList.Select(c =>
        {
            int periodDays = c.Start.eGetDateCountExcludeEnd(c.End);

            return new TariffDetailState
            {
                DateAppliesFrom = c.Start,
                PencePerKWh = c.PeriodPencePerKWh,
                DailyStandingChargePence = (c.PeriodStandingChargePence / periodDays),
                IsHourOfDayFixed = true,
                HourOfDayPrices = ImmutableList<HourOfDayPriceState>.Empty
            };
        }).ToList();

        public static ImmutableList<EnergyBillValues> GasPeriodValuesList = new List<EnergyBillValues>()
        {
            // May
            new EnergyBillValues
            {
                BillDate = BillDate.May,
                TotalCostForPeriodPence = 2128m,
                TotalKWhForPeriod = 132m,
                PeriodPencePerKWh = 9.969m,
                PeriodStandingChargePence = 812m,
                Start = new DateTime(2023, 3, 31),
                End = new DateTime(2023, 4, 30)
            }

        }.ToImmutableList();

        public static ImmutableList<EnergyBillValues> ElectricityBillValuesList = new List<EnergyBillValues>()
        {
            // February
            new EnergyBillValues
            {
                BillDate = BillDate.February,
                TotalCostForPeriodPence = 9232m,
                TotalKWhForPeriod = 242m,
                PeriodPencePerKWh = 32.348m,
                PeriodStandingChargePence = 1404m,
                Start = new DateTime(2022, 12, 31),
                End = new DateTime(2023, 1, 31)
            }, 
            // March
            new EnergyBillValues
            {
                BillDate = BillDate.July,
                TotalCostForPeriodPence = 8644m,
                TotalKWhForPeriod = 228m,
                PeriodPencePerKWh = 32.348m,
                PeriodStandingChargePence = 1269m,
                Start = new DateTime(2023, 1, 31),
                End = new DateTime(2023, 2, 28)
            }, 
            // April
            new EnergyBillValues
            {
                BillDate = BillDate.April,
                TotalCostForPeriodPence = 6224m,
                TotalKWhForPeriod = 149m,
                PeriodPencePerKWh = 32.348m,
                PeriodStandingChargePence = 1404m,
                Start = new DateTime(2023, 2, 28),
                End = new DateTime(2023, 3, 31)
            }, 

            // May
            new EnergyBillValues
            {
                BillDate = BillDate.May,
                TotalCostForPeriodPence = 8016m,
                TotalKWhForPeriod = 205m,
                PeriodPencePerKWh = 31.697m,
                PeriodStandingChargePence = 1518m,
                Start = new DateTime(2023, 3, 31),
                End = new DateTime(2023, 4, 30)
            },

            // June
            new EnergyBillValues
            {
                BillDate = BillDate.June,
                TotalCostForPeriodPence = 10539m,
                TotalKWhForPeriod = 283m,
                PeriodPencePerKWh = 31.697m,
                PeriodStandingChargePence = 1569m,
                Start = new DateTime(2023, 4, 30),
                End = new DateTime(2023, 5, 31)
            }
        }.ToImmutableList();


        public static ImmutableList<TariffDetailState> EnergyTariffsManuallyCreated =>
            EnergyBillValuesManuallyCreated.Select(c =>
            {

                int periodDays = c.Start.eGetDateCountExcludeEnd(c.End);
                return new TariffDetailState
                {
                    DateAppliesFrom = c.Start,
                    PencePerKWh = c.PeriodPencePerKWh,
                    DailyStandingChargePence = c.PeriodStandingChargePence / periodDays,
                    IsHourOfDayFixed = c.IsFixedPencePerKWh,
                    HourOfDayPrices = c.IsFixedPencePerKWh ?
                    ImmutableList<HourOfDayPriceState>.Empty :
                    c.HourOfDayPrices,
                    GlobalId = Guid.NewGuid(),


                };
            }).ToImmutableList();

        public static ImmutableList<EnergyBillValues> EnergyBillValuesManuallyCreated = ImmutableList.Create(
            new EnergyBillValues
            {
                TotalKWhForPeriod = 213.6m,
                PeriodPencePerKWh = 30,
                PeriodStandingChargePence = 1500,
                TotalCostForPeriodPence = 7908m,
                BillDate = BillDate.April,
                Start = new DateTime(2023, 01, 01),
                End = new DateTime(2023, 03, 31),
                IsFixedPencePerKWh = true,
            },
            new EnergyBillValues
            {
                BillDate = BillDate.July,
                TotalKWhForPeriod = 218.4m,
                PeriodPencePerKWh = 25,
                PeriodStandingChargePence = 1000,
                TotalCostForPeriodPence = 6460m,
                Start = new DateTime(2023, 03, 31),
                End = new DateTime(2023, 06, 30),
                IsFixedPencePerKWh = true
            },
            new EnergyBillValues
            {
                BillDate = BillDate.October,
                TotalKWhForPeriod = 220.8m,
                PeriodPencePerKWh = 0,
                PeriodStandingChargePence = 1000,
                TotalCostForPeriodPence = 4477.6m,
                Start = new DateTime(2023, 06, 30),
                End = new DateTime(2023, 09, 30),
                IsFixedPencePerKWh = false,
                HourOfDayPrices = Enumerable.Range(0, 24)
                                .Select(hour =>
                                {
                                    var pencePerKWh = hour >= 7 && hour < 21 ? 20 : 10;
                                    var timeSpan = new TimeSpan(0, hour, 0, 0);
                                    return new HourOfDayPriceState { HourOfDay = timeSpan, PencePerKWh = pencePerKWh };
                                })
                                .ToImmutableList()
            });

    }


    public record EnergyBillValues
    {
        public decimal TotalKWhForPeriod { get; init; }
        public decimal PeriodStandingChargePence { get; init; }
        public decimal PeriodPencePerKWh { get; init; }
        public decimal TotalCostForPeriodPence { get; init; }
        public bool IsFixedPencePerKWh { get; init; }

        public ImmutableList<HourOfDayPriceState> HourOfDayPrices { get; init; }


        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public BillDate BillDate { get; init; }
    }

    public enum BillDate
    {
        February,
        March,
        April,
        May,
        June,
        July,
        October,
    }
}
