using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public static class AnalysisExtensions
    {
        public static string TermAdjective(this CalendarTerm term)
        {
            return term switch
            {
                CalendarTerm.Week => "Weekly",
                CalendarTerm.Day => "Daily",
                CalendarTerm.Month => "Monthly",
                _ => throw new ArgumentOutOfRangeException(nameof(term), term, null)
            };
        }

        public static string AnalysisPeriodHeader(this CalendarTerm duration, RelativePeriod period, DateTime startDate)
        {
            string header;
            if (duration == CalendarTerm.Day)
            {
                header = period switch
                {
                    RelativePeriod.Previous => "Yesterday",
                    RelativePeriod.Current => "Today",
                    RelativePeriod.Next => "Tomorrow",
                    RelativePeriod.Historical => startDate.eDateToMinimal(),
                    _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
                };
                return header;
            }

            if (period == RelativePeriod.Historical)
            {
                switch (duration)
                {
                    case CalendarTerm.Week:
                        return $"Week Starting {startDate.eDateToMinimal()}";
                    case CalendarTerm.Month:
                        return startDate.ToString("M");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(duration), duration, null);
                }
            }

            header = period switch
            {
                RelativePeriod.Previous => "Last ",
                RelativePeriod.Current => "This ",
                RelativePeriod.Next => "Next ",
                _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
            };

            header += duration.ToString();
            return header;
        }
    }
}
