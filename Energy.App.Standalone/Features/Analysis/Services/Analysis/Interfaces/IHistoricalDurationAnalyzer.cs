using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

public interface IHistoricalDurationAnalyzer
{
    HistoricalAnalysis GetPreviousDurationAnalysis(MeterType meterType, CalendarTerm duration);
    HistoricalAnalysis GetCurrentDurationAnalysis(MeterType meterType, CalendarTerm duration);
}