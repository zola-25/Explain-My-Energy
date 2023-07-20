using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

public interface IHistoricalConsumptionSummarizer
{
    HistoricalAnalysis GetCurrentDurationAnalysis(MeterType meterType,
                                                  CalendarTerm term);
    HistoricalAnalysis GetPreviousDurationAnalysis(MeterType meterType,
                                                   CalendarTerm term);
}