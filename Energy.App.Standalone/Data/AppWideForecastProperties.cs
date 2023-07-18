using Energy.App.Standalone.Extensions;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Data
{
    public static class AppWideForecastProperties
    {
        public static readonly ImmutableList<int> LowTemperatureMonths = ImmutableList.Create(10, 11, 12, 1, 2, 3, 4);

        public static int GetLowTemperatureDays(DateTime latestDate)
        {
            int days = 0;
            foreach (var month in LowTemperatureMonths)
            {
                if (latestDate.Month <= month)
                {
                    int year = latestDate.Year - 1;
                    days += DateTime.DaysInMonth(year, month);
                }
                else
                {
                    int year = latestDate.Year;
                    days += DateTime.DaysInMonth(year, month);
                }
            }
            return days;
        }

        public static int ForecastFromDays = -60;
        public static int ForecastFutureDays = 180;

        public static DateTime PredictionStartDate(DateTime latestReadingDate)
        {
            return latestReadingDate.AddTicks(ForecastFromDays * TimeSpan.TicksPerDay).Date;
        }

        public static DateTime PredictionEndDate(DateTime latestReadingDate)
        {
            return latestReadingDate.AddTicks((ForecastFutureDays) * TimeSpan.TicksPerDay).Date;
        }

        public static IEnumerable<DateTime> PredictionDates(DateTime latestReadingDate)
        {
            var startDate = PredictionStartDate(latestReadingDate);
            var endDate = PredictionEndDate(latestReadingDate);
            return startDate.eGenerateAllDatesBetween(endDate, true);
        }

        public static int MovingAverageWindowSizeDays = 30;
        public static int MovingAverageWindowSizeHalfHours = 48 * MovingAverageWindowSizeDays;

    }
}
