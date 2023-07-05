using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.Shared;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        
        public static List<TariffDetailState> TestElectricityTariffs => ElectricityBillValuesList.Select(c =>
        {
            int periodDays = c.Start.eGetDateCountExcludeEnd(c.End);

            return new TariffDetailState
            {
                DateAppliesFrom = c.Start,
                PencePerKWh = c.PeriodPencePerKWh,
                DailyStandingChargePence = (c.PeriodStandingChargePence / periodDays),
                IsHourOfDayFixed = true,
                HourOfDayPrices = new List<HourOfDayPriceState>()
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
                HourOfDayPrices = new List<HourOfDayPriceState>()
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
                BillDate = BillDate.March,
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

    }


    public record EnergyBillValues
    {
        public decimal TotalKWhForPeriod { get; init; }
        public decimal PeriodStandingChargePence { get; init; }
        public decimal PeriodPencePerKWh { get; init; }
        public decimal TotalCostForPeriodPence { get; init; }
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
    }

}
