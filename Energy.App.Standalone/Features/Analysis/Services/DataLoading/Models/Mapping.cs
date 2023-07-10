using Energy.App.Standalone.Extensions;

namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models
{
    public static class Mapping
    {
        public static ChartReading MapToChartReading(CostedReading costedReading)
        {
            return new ChartReading()
            {
                Cost = costedReading.CostP,
                KWh = costedReading.KWh,
                PencePerKWh = costedReading.TPpKWh,
                DailyStandingCharge = costedReading.TDStndP,
                DateTicks = costedReading.UtcTime.eToUnixTicksNoOffset(),
                HalfHourlyStandingCharge = costedReading.THHStndCh,
                TariffAppliesFrom = costedReading.TApFrom,
                TariffType = String.Empty,
                IsForecast = costedReading.Fcst
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
