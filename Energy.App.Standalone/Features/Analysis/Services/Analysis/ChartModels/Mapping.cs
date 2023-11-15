using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.ChartModels;

public static class Mapping
{
    public static ChartReading MapToChartReading(CostedReading costedReading)
    {
        return new ChartReading()
        {
            Cost = costedReading.CostPounds,
            KWh = costedReading.KWh,
            PencePerKWh = costedReading.TariffPencePerKWh,
            DailyStandingCharge = costedReading.TariffDailyStandingCharge,
            DateTicks = costedReading.UtcTime.eToUnixTicksNoOffset(),
            HalfHourlyStandingCharge = costedReading.TariffHalfHourlyStandingCharge,
            TariffAppliesFrom = costedReading.TariffAppliesFrom,
            TariffType = costedReading.IsFixedCostPerHour ? "Fixed Rate" : "Variable Rate",
        };
    }

    public static ChartDailyForecastReading MapToChartReading(DailyCostedReading costedReading)
    {
        return new ChartDailyForecastReading()
        {
            Cost = costedReading.ReadingTotalCostPounds,
            KWh = costedReading.KWh,
            PencePerKWh = costedReading.PencePerKWh,
            DailyStandingCharge = costedReading.TariffDailyStandingChargePence,
            DateTicks = costedReading.UtcTime.eToUnixTicksNoOffset(),
            TariffAppliesFrom = costedReading.TariffAppliesFrom
        };
    }
}
