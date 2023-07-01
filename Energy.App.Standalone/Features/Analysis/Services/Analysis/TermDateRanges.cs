using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public class TermDateRanges : ITermDateRanges
    {
        public (DateTime start, DateTime end) GetPreviousPeriodDates(CalendarTerm duration)
        {
            DateTime end = GetCurrentPeriodStartingDate(duration);
            DateTime start = duration switch
            {
                CalendarTerm.Week => end.AddDays(-7),
                CalendarTerm.Day => end.AddDays(-1),
                CalendarTerm.Month => end.AddMonths(-1),
                _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
            };

            return (start, end);
        }

        public (DateTime start, DateTime end) GetCurrentPeriodDates(CalendarTerm duration)
        {
            DateTime start = GetCurrentPeriodStartingDate(duration);
            DateTime end = duration switch
            {
                CalendarTerm.Week => start.AddDays(7),
                CalendarTerm.Month => start.AddMonths(1),
                CalendarTerm.Day => start.AddDays(1),
                _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
            };
            return (start, end);
        }

        public (DateTime start, DateTime end) GetNextPeriodDates(CalendarTerm duration)
        {
            DateTime start = GetNextPeriodStartingDate(duration);
            DateTime end = GetNextPeriodEndDate(duration, start);
            return (start, end);
        }

        public DateTime GetCurrentPeriodStartingDate(CalendarTerm duration)
        {
            DateTime start = duration switch
            {
                CalendarTerm.Week => GetThisWeeksStartingDate(),
                CalendarTerm.Month => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                CalendarTerm.Day => DateTime.Today,
                _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
            };

            return start;
        }

        private DateTime GetThisWeeksStartingDate(DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            DateTime start = DateTime.Today;
            while (start.DayOfWeek != startOfWeek)
            {
                start = start.AddDays(-1).Date;
            }
            return start;
        }

        private DateTime GetNextPeriodStartingDate(CalendarTerm duration)
        {
            DateTime today = DateTime.Today;
            DateTime start = duration switch
            {
                CalendarTerm.Week => GetNextWeekStartingDate(),
                CalendarTerm.Day => DateTime.Today.AddDays(1),
                CalendarTerm.Month => new DateTime(today.Year, today.Month + 1, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
            };

            return start;
        }

        private DateTime GetNextPeriodEndDate(CalendarTerm duration, DateTime start)
        {
            return duration switch
            {
                CalendarTerm.Week => start.AddDays(7).Date,
                CalendarTerm.Day => start.AddDays(1).Date,
                CalendarTerm.Month => start.AddMonths(1),
                _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
            };
        }
        private DateTime GetNextWeekStartingDate(DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            DateTime start = DateTime.Today.AddDays(1).Date;
            while (start.DayOfWeek != startOfWeek)
            {
                start = start.AddDays(1).Date;
            }
            return start;
        }
    }
}
