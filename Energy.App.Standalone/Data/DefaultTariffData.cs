using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;

namespace Energy.App.Standalone.Data
{
    public static class DefaultTariffData
    {
        public static DefaultTariffDetail GetCurrentExampleTariff(MeterType meterType, ExampleTariffType exampleTariffType)
        {
            return DefaultTariffs.Where(c => c.MeterType == meterType && c.ExampleTariffType == exampleTariffType && DateTime.Today >= c.DateAppliesFrom)
                .OrderByDescending(c => c.DateAppliesFrom)
                .First();
        }

        public static readonly List<DefaultTariffDetail> DefaultTariffs = new List<DefaultTariffDetail>
        {
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
}
