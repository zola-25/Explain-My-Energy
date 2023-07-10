using Energy.App.Standalone.Extensions;

namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models
{
    public static class Mapping
    {
        public static ChartReading MapToChartReading(CostedReading costedReading)
        {
            return new ChartReading()
            {
                Cost = costedReading.CostPence,
                KWh = costedReading.KWh,
                PencePerKWh = costedReading.TariffPencePerKWh,
                DailyStandingCharge = costedReading.TariffDailyStandingCharge,
                DateTicks = costedReading.UtcTime.eToUnixTicksNoOffset(),
                HalfHourlyStandingCharge = costedReading.TariffHalfHourlyStandingCharge,
                TariffAppliesFrom = costedReading.TarrifAppliesFrom,
                TariffType = String.Empty,
                IsForecast = costedReading.IsForecast
            };
        }

        public static ChartDailyForecastReading MapToChartReading(DailyCostedReading costedReading)
        {
            return new ChartDailyForecastReading()
            {
                Cost = costedReading.ReadingTotalCostPence,
                KWh = costedReading.KWh,
                PencePerKWh = costedReading.PencePerKWh,
                DailyStandingCharge = costedReading.TariffDailyStandingChargePence,
                DateTicks = costedReading.UtcTime.eToUnixTicksNoOffset(),
                TariffAppliesFrom = costedReading.TariffAppliesFrom
            };
        }
    }
}
