using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

public interface IHistoricalForecastSummarizer
{
    ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType, CalendarTerm term);
    ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType, CalendarTerm term);
}