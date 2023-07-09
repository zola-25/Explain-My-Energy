using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.Shared;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;



public interface ITempForecastAnalyzer
{
    ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType, CalendarTerm term, decimal degreeDifference);
    ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType, CalendarTerm term, decimal degreeDifference);
}

public interface ISimpleForecastAnalyzer
{

}