using Energy.App.Standalone.Features.Analysis.Services.Analysis.AnalysisModels;
using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;



public interface ITempForecastSummarizer
{
    ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType, CalendarTerm term, bool useHistorical);
    ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType, CalendarTerm term, bool useHistorical);
}
