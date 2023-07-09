using Energy.App.Standalone.Extensions;

namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models
{
    public static class Mapping
    {
        public static ChartReading MapToChartReading(CostedReading costedReading)
        {
            return new ChartReading()
            {
                Cost = costedReading.ReadingTotalCostPence,
                KWh = costedReading.KWh,
                PencePerKWh = costedReading.TariffHalfHourlyPencePerKWh,
                DailyStandingCharge = costedReading.TariffDailyStandingChargePence,
                DateTicks = costedReading.UtcTime.eToUnixTicksNoOffset(),
                HalfHourlyStandingCharge = costedReading.TariffHalfHourlyStandingChargePence,
                TariffAppliesFrom = costedReading.TariffAppliesFrom,
                TariffType = String.Empty,
                IsForecast = costedReading.Forecast
            };
        }
    }
}
