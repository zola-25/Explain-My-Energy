using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public static class AnalysisExtensions
    {
        public static int ToInt(this decimal value)
        {
            return Convert.ToInt32(value);
        }
        
        public static int NumberOfDecimals(this CalendarTerm term)
        {
            return term switch
            {
                CalendarTerm.Week => 1,
                CalendarTerm.Day => 2,
                CalendarTerm.Month => 0,
                _ => throw new ArgumentOutOfRangeException(nameof(term), term, null)
            };
        }
        
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
        
        public static string AnalysisPeriodTemperatureText (this CalendarTerm duration, RelativePeriod period, TemperatureRange temperatureRange)
        {
            string temperatureText;
            if (duration == CalendarTerm.Day)
            {
                temperatureText = period switch
                {
                    RelativePeriod.Previous => $"{temperatureRange.AverageTemp}{temperatureRange.Symbol}",
                    RelativePeriod.Current => $"{temperatureRange.AverageTemp}{temperatureRange.Symbol}",
                    RelativePeriod.Next => $"{temperatureRange.AverageTemp}{temperatureRange.Symbol}",
                    _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
                };
                return temperatureText;
            }

            temperatureText = $"{temperatureRange.LowDailyTemp} - {temperatureRange.HighDailyTemp}{temperatureRange.Symbol}";
            return temperatureText;
        }

        public static string AnalysisPeriodDateRangeText(this CalendarTerm duration, RelativePeriod period, DateTime startDate, DateTime endDate)
        {
            string rangeText;
            if (duration == CalendarTerm.Day)
            {
                rangeText = period switch
                {
                    RelativePeriod.Previous => $"{startDate.eDateToMinimal()}",
                    RelativePeriod.Current => $"{startDate.eDateToMinimal()}",
                    RelativePeriod.Next => $"{startDate.eDateToMinimal()}",
                    _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
                };
                return rangeText;
            }

            rangeText = $"{startDate.eDateToMinimal()} - {endDate.eDateToMinimal()}";
            return rangeText;
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
                    _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
                };
                return header;
            }

            // if (period == RelativePeriod.Historical)
            // {
            //     switch (duration)
            //     {
            //         case CalendarTerm.Week:
            //             return $"Week Starting {startDate.eDateToMinimal()}";
            //         case CalendarTerm.Month:
            //             return startDate.ToString("M");
            //         default:
            //             throw new ArgumentOutOfRangeException(nameof(duration), duration, null);
            //     }
            // }

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
