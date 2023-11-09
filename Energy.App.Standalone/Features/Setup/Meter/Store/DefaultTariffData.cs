using Energy.Shared;
using System.Collections.Immutable;
using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.App.Standalone.DTOs.Tariffs;

namespace Energy.App.Standalone.Features.Setup.Meter.Store;

public static class DefaultTariffData
{
    public static DefaultTariffDetail GetCurrentExampleTariff(MeterType meterType, ExampleTariffType exampleTariffType)
    {
        return DefaultTariffs.Where(c => c.MeterType == meterType && c.ExampleTariffType == exampleTariffType && DateTime.Today >= c.DateAppliesFrom)
            .OrderByDescending(c => c.DateAppliesFrom)
            .First();
    }

    public static ImmutableList<TariffDetailState> GetDefaultTariffs(MeterType meterType,
                                                                       ExampleTariffType exampleTariffType)
    {
        return DefaultTariffs
            .Where(c => c.MeterType == meterType && c.ExampleTariffType == exampleTariffType)
            .Select(c => c.eMapDefaultTariff())
            .ToImmutableList();
    }

    public static readonly List<DefaultTariffDetail> DefaultTariffs = new() {
            // Direct Debit Price Cap 1st Oct 2023 - 31st Dec 2023 - https://www.moneysavingexpert.com/news/2021/08/energy-price-cap-to-rise-by--139---but-you-can-save--200-by-switch/
            new()
            {
                IsHourOfDayFixed = true,
                MeterType = MeterType.Electricity,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                PencePerKWh = 27.35m,
                DateAppliesFrom = new DateTime(2023, 10, 01),
                DailyStandingChargePence = 53.37m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(27.35m).ToList(),
            },
            new()
            {
                IsHourOfDayFixed = true,
                MeterType = MeterType.Gas,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                PencePerKWh = 6.89m,
                DateAppliesFrom = new DateTime(2023, 10, 01),
                DailyStandingChargePence = 29.62m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(6.89m).ToList(),

            },
            // Direct Debit Price Cap 1st July - 30th Sept 2023 - https://www.moneysavingexpert.com/news/2023/08/energy-bills-to-fall-as-new-price-cap-is-announced---what-you-ne/
            new()
            {
                IsHourOfDayFixed = true,
                MeterType = MeterType.Electricity,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                PencePerKWh = 30.11m,
                DateAppliesFrom = new DateTime(2023, 07, 01),
                DailyStandingChargePence = 52.97m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(30.11m).ToList(),
            },
            new()
            {
                IsHourOfDayFixed = true,
                MeterType = MeterType.Gas,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                PencePerKWh = 7.51m,
                DateAppliesFrom = new DateTime(2023, 07, 01),
                DailyStandingChargePence = 29.11m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(7.51m).ToList(),

            },
            new()
            {
                IsHourOfDayFixed = true,
                MeterType = MeterType.Electricity,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                PencePerKWh = 40.8m,
                DateAppliesFrom = new DateTime(2023, 04, 01),
                DailyStandingChargePence = 55.2m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(40.8m).ToList(),
            },
            new()
            {
                IsHourOfDayFixed = true,
                MeterType = MeterType.Gas,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                PencePerKWh = 12.36m,
                DateAppliesFrom = new DateTime(2023, 04, 01),
                DailyStandingChargePence = 33.6m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(12.36m).ToList(),

            },
            // Oct -Apr
            new()
            {
                IsHourOfDayFixed = true,
                MeterType = MeterType.Electricity,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                PencePerKWh = 34m,
                DateAppliesFrom = new DateTime(2022, 10, 01),
                DailyStandingChargePence = 46m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(34m).ToList(),
            },

            new()
            {
                IsHourOfDayFixed = true,
                MeterType = MeterType.Gas,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                PencePerKWh = 10.3m,
                DateAppliesFrom = new DateTime(2022, 10, 01),
                DailyStandingChargePence = 28m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(10.3m).ToList(),
            },
            // Apr - Oct 22
            new()
            {
                IsHourOfDayFixed = true,
                MeterType = MeterType.Electricity,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                PencePerKWh = 28.3m,
                DateAppliesFrom = new DateTime(2022, 04, 01),
                DailyStandingChargePence = 45m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(28.3m).ToList(),

            },
            new()
            {
                IsHourOfDayFixed = true,
                MeterType = MeterType.Gas,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                PencePerKWh = 7.37m,
                DateAppliesFrom = new DateTime(2022, 04, 01),
                DailyStandingChargePence = 23m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(7.37m).ToList(),

            },
            //Oct 21 - Apr 22
            new()
            {
                IsHourOfDayFixed = true,
                MeterType = MeterType.Electricity,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                PencePerKWh = 20.8m,
                DateAppliesFrom = new DateTime(2021, 10, 01),
                DailyStandingChargePence = 23m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(20.8m).ToList(),

            },
            new()
            {
                IsHourOfDayFixed = true,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                MeterType = MeterType.Gas,
                PencePerKWh = 4.1m,
                DateAppliesFrom = new DateTime(2021, 10, 01),
                DailyStandingChargePence = 24m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(4.1m).ToList(),

            },
            // Apr 21 - Oct 21
            new()
            {
                IsHourOfDayFixed = true,
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                MeterType = MeterType.Electricity,
                PencePerKWh = 19m,
                DateAppliesFrom = new DateTime(2021, 04, 01),
                DailyStandingChargePence = 23m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(19m).ToList(),

            },
            new()
            {
                ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                IsHourOfDayFixed = true,
                MeterType = MeterType.Gas,
                PencePerKWh = 3.3m,
                DateAppliesFrom = new DateTime(2021, 04, 01),
                DailyStandingChargePence = 25m,
                DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(3.3m).ToList(),

            },
            new()
            {
                IsHourOfDayFixed = false,
                MeterType = MeterType.Electricity,
                ExampleTariffType = ExampleTariffType.Economy7Variable,
                DateAppliesFrom = new DateTime(2022, 01, 01),
                DailyStandingChargePence = 38m,
                DefaultHourOfDayPrices = CreateExampleEconomy7Prices(34m, 19m).ToList(),
            },
            new()
            {
                IsHourOfDayFixed = false,
                MeterType = MeterType.Electricity,
                ExampleTariffType = ExampleTariffType.Economy7Variable,
                DateAppliesFrom = new DateTime(2021, 01, 01),
                DailyStandingChargePence = 24m,
                DefaultHourOfDayPrices = CreateExampleEconomy7Prices(22m, 11m).ToList(),
            },
        };


    private static IEnumerable<DefaultHourOfDayPrice> CreateExampleEconomy7Prices(decimal dayRate, decimal nightRate)
    {
        return Enumerable.Range(0, 24).Select(i => new DefaultHourOfDayPrice()
        {
            PencePerKWh = i < 7 ? nightRate : dayRate,
            HourOfDay = TimeSpan.FromHours(i),
        });
    }

    private static IEnumerable<DefaultHourOfDayPrice> CreateUniform24HourOfDayPrices(decimal fixedPencePerKWh)
    {
        return Enumerable.Range(0, 24).Select(i => new DefaultHourOfDayPrice()
        {
            PencePerKWh = fixedPencePerKWh,
            HourOfDay = TimeSpan.FromHours(i),
        });
    }
}
