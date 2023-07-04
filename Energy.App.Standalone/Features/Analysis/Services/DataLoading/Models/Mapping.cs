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
                DateTicks = costedReading.UtcTime.eToUnixTime(),
                HalfHourlyStandingCharge = costedReading.TariffHalfHourlyStandingChargePence,
                TariffAppliesFrom = costedReading.TariffAppliesFrom,
                IsForecast = costedReading.Forecast
            };
        }
    }
}
