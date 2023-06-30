using TimeZoneConverter;

namespace Energy.Shared;

public static class AppDefaults
{
    public static TimeZoneInfo GetUkTimezone()
    {
        TimeZoneInfo ukTimezone = GetTimeZoneById("GMT Standard Time");
        return ukTimezone;
    }

    public static TimeZoneInfo GetTimeZoneById(string timeZoneId)
    {
        return TZConvert.GetTimeZoneInfo(timeZoneId);
    }

    //public static IEnumerable<HourOfDayPrice> GetDefaultElectricityHourOfDayCosts()
    //{
    //    return Enumerable.Range(0, 48)
    //        .Select(c => new HourOfDayPrice
    //        {
    //            HourOfDay = new TimeSpan(c / 2, c % 2 == 0 ? 0 : 30, 0),
    //            PencePerKWh = 0.3235
    //        });
    //}

    //public static IEnumerable<HourOfDayPrice> GetDefaultGasHourOfDayCosts()
    //{
    //    return Enumerable.Range(0, 48)
    //        .Select(c => new HourOfDayPrice
    //        {
    //            HourOfDay = new TimeSpan(c / 2, c % 2 == 0 ? 0 : 30, 0),
    //            PencePerKWh = 0.0989
    //        });
    //}

    //public static Dictionary<DayOfWeek, int> WeekdayOrderingMondayFirst
    //    => new Dictionary<DayOfWeek, int>
    //    {
    //            {DayOfWeek.Monday, 0 },
    //            {DayOfWeek.Tuesday, 1 },
    //            {DayOfWeek.Wednesday, 2 },
    //            {DayOfWeek.Thursday, 3 },
    //            {DayOfWeek.Friday, 4 },
    //            {DayOfWeek.Saturday, 5 },
    //            {DayOfWeek.Sunday, 6 },
    //            {DayOfWeek.BankHoliday, 7 },

    //    };

    //public static double PredictionBoundaryLimit => 0.9;

    //public static DateTime MinimumSupportedDate => new DateTime(2016, 1, 1);

    //public static int HomeOfficePortfolioId => 1001;

    //public static int NumberOfClientPlaceholders => 3;
    //public static int DefaultMaxConcurrency { get; set; } = 10;


    //public static int GetDefaultCalendarId()
    //{
    //    return 1;
    //}
}
