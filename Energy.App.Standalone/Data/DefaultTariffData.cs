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
                    PencePerKWh = 40.8,
                    DateAppliesFrom = new DateTime(2023, 04, 01),
                    DailyStandingChargePence = 55.2,
                    DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(40.8).ToList(),
                },
                new()
                {
                    IsHourOfDayFixed = true,
                    MeterType = MeterType.Gas,
                    ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                    PencePerKWh = 12.36,
                    DateAppliesFrom = new DateTime(2023, 04, 01),
                    DailyStandingChargePence = 33.6,
                    DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(12.36).ToList(),

                },
                // Oct -Apr
                new()
                {
                    IsHourOfDayFixed = true,
                    MeterType = MeterType.Electricity,
                    ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                    PencePerKWh = 34,
                    DateAppliesFrom = new DateTime(2022, 10, 01),
                    DailyStandingChargePence = 46,
                    DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(34).ToList(),
                },

                new()
                {
                    IsHourOfDayFixed = true,
                    MeterType = MeterType.Gas,
                    ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                    PencePerKWh = 10.3,
                    DateAppliesFrom = new DateTime(2022, 10, 01),
                    DailyStandingChargePence = 28,
                    DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(10.3).ToList(),
                },
                // Apr - Oct 22
                new()
                {
                    IsHourOfDayFixed = true,
                    MeterType = MeterType.Electricity,
                    ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                    PencePerKWh = 28.3,
                    DateAppliesFrom = new DateTime(2022, 04, 01),
                    DailyStandingChargePence = 45,
                    DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(28.3).ToList(),

                },
                new()
                {
                    IsHourOfDayFixed = true,
                    MeterType = MeterType.Gas,
                    ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                    PencePerKWh = 7.37,
                    DateAppliesFrom = new DateTime(2022, 04, 01),
                    DailyStandingChargePence = 23,
                    DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(7.37).ToList(),

                },
                //Oct 21 - Apr 22
                new()
                {
                    IsHourOfDayFixed = true,
                    MeterType = MeterType.Electricity,
                    ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                    PencePerKWh = 20.8,
                    DateAppliesFrom = new DateTime(2021, 10, 01),
                    DailyStandingChargePence = 23,
                    DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(20.8).ToList(),

                },
                new()
                {
                    IsHourOfDayFixed = true,
                    ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                    MeterType = MeterType.Gas,
                    PencePerKWh = 4.1,
                    DateAppliesFrom = new DateTime(2021, 10, 01),
                    DailyStandingChargePence = 24,
                    DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(4.1).ToList(),

                },
                // Apr 21 - Oct 21
                new()
                {
                    IsHourOfDayFixed = true,
                    ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                    MeterType = MeterType.Electricity,
                    PencePerKWh = 19,
                    DateAppliesFrom = new DateTime(2021, 04, 01),
                    DailyStandingChargePence = 23,
                    DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(19).ToList(),

                },
                new()
                {
                    ExampleTariffType = ExampleTariffType.StandardFixedDaily,
                    IsHourOfDayFixed = true,
                    MeterType = MeterType.Gas,
                    PencePerKWh = 3.3,
                    DateAppliesFrom = new DateTime(2021, 04, 01),
                    DailyStandingChargePence = 25,
                    DefaultHourOfDayPrices = CreateUniform24HourOfDayPrices(3.3).ToList(),

                },
                new()
                {
                    IsHourOfDayFixed = false,
                    MeterType = MeterType.Electricity,
                    ExampleTariffType = ExampleTariffType.Economy7Variable,
                    DateAppliesFrom = new DateTime(2022, 01, 01),
                    DailyStandingChargePence = 38,
                    DefaultHourOfDayPrices = CreateExampleEconomy7Prices(34, 19).ToList(),
                },
                new()
                {
                    IsHourOfDayFixed = false,
                    MeterType = MeterType.Electricity,
                    ExampleTariffType = ExampleTariffType.Economy7Variable,
                    DateAppliesFrom = new DateTime(2021, 01, 01),
                    DailyStandingChargePence = 24,
                    DefaultHourOfDayPrices = CreateExampleEconomy7Prices(22, 11).ToList(),
                },
            };


        private static IEnumerable<DefaultHourOfDayPrice> CreateExampleEconomy7Prices(double dayRate, double nightRate)
        {
            return Enumerable.Range(0, 24).Select(i => new DefaultHourOfDayPrice()
            {
                PencePerKWh = i < 7 ? nightRate : dayRate,
                HourOfDay = TimeSpan.FromHours(i),
            });
        }

        private static IEnumerable<DefaultHourOfDayPrice> CreateUniform24HourOfDayPrices(double fixedPencePerKWh)
        {
            return Enumerable.Range(0, 24).Select(i => new DefaultHourOfDayPrice()
            {
                PencePerKWh = fixedPencePerKWh,
                HourOfDay = TimeSpan.FromHours(i),
            });
        }
    }
}
