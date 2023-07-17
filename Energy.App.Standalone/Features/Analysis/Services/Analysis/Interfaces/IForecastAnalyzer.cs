using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;



public interface ITempForecastAnalyzer
{
    ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType, CalendarTerm term, bool useHistorical);
    ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType, CalendarTerm term, bool useHistorical);
}

public interface ISimpleForecastAnalyzer
{
    ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType, CalendarTerm term);
    ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType, CalendarTerm term);
}