using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

public interface IForecastAnalyzer
{
    ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType,
        CalendarTerm duration);

    ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType,
        CalendarTerm duration);

}

public interface ITempForecastAnalyzer : IForecastAnalyzer { }

public interface ISimpleForecastAnalyzer : IForecastAnalyzer { }