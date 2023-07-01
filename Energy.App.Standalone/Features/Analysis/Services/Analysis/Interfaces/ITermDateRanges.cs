using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

public interface ITermDateRanges
{
    (DateTime start, DateTime end) GetCurrentPeriodDates(CalendarTerm duration);
    (DateTime start, DateTime end) GetPreviousPeriodDates(CalendarTerm duration);
    (DateTime start, DateTime end) GetNextPeriodDates(CalendarTerm duration);
    DateTime GetCurrentPeriodStartingDate(CalendarTerm duration);
}