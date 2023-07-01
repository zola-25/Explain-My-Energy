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
                PencePerKWh = costedReading.PencePerKWh,
                DailyStandingCharge = costedReading.DailyStandingChargePence,
                DateTicks = costedReading.LocalTime.eToUnixTime(),
                HalfHourlyStandingCharge = costedReading.HalfHourlyStandingCharge,
                TariffType = costedReading.TariffType.ToString(),
                TariffAppliesFrom = costedReading.TariffAppliesFrom,
                IsForecast = costedReading.Forecast
            };
        }
    }
}
