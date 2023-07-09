using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public class TermDateRanges : ITermDateRanges
    {
        public TermDateRanges(DateTime today)
        {
            Today = DateTime.SpecifyKind(today, DateTimeKind.Utc);
        }

        public TermDateRanges()
        {
            Today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        }

        public DateTime Today { get; set; }

        public (DateTime start, DateTime end) GetPreviousPeriodDates(CalendarTerm duration)
        {
            DateTime end = GetCurrentPeriodStartingDate(duration).AddDays(-1);
            DateTime start = duration switch
            {
                CalendarTerm.Week => end.AddDays(-6),
                CalendarTerm.Day => end,
                CalendarTerm.Month => new DateTime(end.Year, end.Month, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
            };
            
            end = DateTime.SpecifyKind(end, DateTimeKind.Utc);
            return (start, end);
        }

        public (DateTime start, DateTime end) GetCurrentPeriodDates(CalendarTerm duration)
        {
            DateTime start = GetCurrentPeriodStartingDate(duration);
            DateTime end = duration switch
            {
                CalendarTerm.Week => start.AddDays(6),
                CalendarTerm.Month => start.AddMonths(1).AddDays(-1),
                CalendarTerm.Day => start,
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
                CalendarTerm.Day => Today,
                CalendarTerm.Month => new DateTime(Today.Year, Today.Month, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
            };
            
            return DateTime.SpecifyKind(start, DateTimeKind.Utc);
        }

        private DateTime GetThisWeeksStartingDate(DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            DateTime start = Today;
            while (start.DayOfWeek != startOfWeek)
            {
                start = start.AddDays(-1).Date;
            }
            return start;
        }

        private DateTime GetNextPeriodStartingDate(CalendarTerm duration)
        {
            DateTime today = Today;
            DateTime start = duration switch
            {
                CalendarTerm.Week => GetNextWeekStartingDate(),
                CalendarTerm.Day => today.AddDays(1),
                CalendarTerm.Month => new DateTime(today.Year, today.Month + 1, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
            };

            return DateTime.SpecifyKind(start, DateTimeKind.Utc);
        }

        private DateTime GetNextPeriodEndDate(CalendarTerm duration, DateTime start)
        {
            var end = duration switch
            {
                CalendarTerm.Week => start.AddDays(6),
                CalendarTerm.Day => start,
                CalendarTerm.Month => start.AddMonths(1).AddDays(-1),
                _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
            };
            
            return DateTime.SpecifyKind(end, DateTimeKind.Utc);
        }
        private DateTime GetNextWeekStartingDate(DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            DateTime start = Today.AddDays(1).Date;
            while (start.DayOfWeek != startOfWeek)
            {
                start = start.AddDays(1).Date;
            }
            return start;
        }
    }
}
