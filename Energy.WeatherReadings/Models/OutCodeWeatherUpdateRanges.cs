using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Energy.WeatherReadings.Models;

public record OutCodeWeatherUpdateRanges
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string OutCode { get; init; }

    public DateTime? LatestHistoricalUtc { get; init; }

    public DateTime? LatestReadingUtc { get; init; }

    public bool HasAnyReadings => LatestReadingUtc.HasValue;
    public DateTime UtcToday { get; init; }

    public (DateTime start, DateTime end) GetNearTermForecastRange()
    {
        return (UtcToday.AddDays(-59), UtcToday.AddDays(14));
    }

    public (DateTime start, DateTime end) GetLongTermForecastRange()
    {
        return (UtcToday.AddDays(15), UtcToday.AddMonths(6));
    }

    public (DateTime start, DateTime end, bool update) GetHistoricalRange()
    {

        DateTime utcToday = UtcToday;
        DateTime utcTwoMonthsAgo = utcToday.AddDays(-60).Date;
        DateTime oneYearAnd30DaysAgo = utcToday.AddYears(-1).AddDays(-30).Date;

        if (!HasAnyReadings)
        {
            return (oneYearAnd30DaysAgo, utcTwoMonthsAgo, true);
        }

        if (!LatestHistoricalUtc.HasValue) // no historical values? Make them until the last two months
        {
            return (oneYearAnd30DaysAgo, utcTwoMonthsAgo, true);
        }

        if (LatestHistoricalUtc < utcTwoMonthsAgo)
        {
            return (LatestHistoricalUtc.Value, utcTwoMonthsAgo, true);
        }

        return (LatestHistoricalUtc.Value, LatestHistoricalUtc.Value, false);

    }


}
