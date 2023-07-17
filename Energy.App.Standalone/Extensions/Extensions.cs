using Energy.Shared;
using Microsoft.AspNetCore.Components;
using System.Collections.Immutable;
using System.Text;
using Energy.App.Standalone.Features.Setup.Meter.Store.StateObjects;
using Energy.App.Standalone.DTOs;
using Energy.App.Standalone.DTOs.Tariffs;

namespace Energy.App.Standalone.Extensions;

public static class Extensions
{
#pragma warning disable IDE1006 // Naming Styles
    public static bool eIsNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        return enumerable == null || !enumerable.Any();
    }

    public static bool eIsNotNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        return enumerable != null && enumerable.Any();
    }

    public static void eLogToConsole(this ComponentBase component, string action)
    {
        Console.WriteLine($"{component.GetType().Name}: {action}");
    }

    public static string eTimeSpanToString(this TimeSpan timeSpan)
    {
        return string.Format($"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}");
    }

    public static string eTimeSpanToString(this TimeSpan? nullableTimeSpan)
    {
        TimeSpan timeSpan = nullableTimeSpan ?? TimeSpan.Zero;
        return string.Format($"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}");
    }

    public static long eToUnixTime(this DateTime dateTime)
    {
        TimeZoneInfo timeZone = AppDefaults.GetUkTimezone();
        return new DateTimeOffset(dateTime, timeZone.GetUtcOffset(dateTime)).ToUnixTimeMilliseconds();
    }

    public static long eToUnixTicksNoOffset(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
    }


    public static List<HourOfDayPriceState> eMapToHourOfDayPriceState(this IEnumerable<DefaultHourOfDayPrice> hourOfDayPriceStates)
    {
        return hourOfDayPriceStates.Select(c => new HourOfDayPriceState
        {
            HourOfDay = c.HourOfDay,
            PencePerKWh = c.PencePerKWh
        }).ToList();
    }

    public static List<HourOfDayPrice> eMapToHourOfDayPriceDto(this IEnumerable<HourOfDayPriceState> hourOfDayPriceStates)
    {
        return hourOfDayPriceStates.Select(c => new HourOfDayPrice
        {
            HourOfDay = c.HourOfDay,
            PencePerKWh = c.PencePerKWh
        }).ToList();
    }

    public static List<HourOfDayPriceState> eMapToHourOfDayPriceState(this IEnumerable<HourOfDayPrice> hourOfDayPriceDtos)
    {
        return hourOfDayPriceDtos.Select(c => new HourOfDayPriceState
        {
            HourOfDay = c.HourOfDay,
            PencePerKWh = c.PencePerKWh
        }).ToList();
    }

    public static TariffDetailState eMapDefaultTariff(this DefaultTariffDetail defaultTariffDetail)
    {
        return new TariffDetailState
        {
            GlobalId = Guid.NewGuid(),
            PencePerKWh = defaultTariffDetail.PencePerKWh,
            DailyStandingChargePence = defaultTariffDetail.DailyStandingChargePence,
            DateAppliesFrom = defaultTariffDetail.DateAppliesFrom,
            IsHourOfDayFixed = defaultTariffDetail.IsHourOfDayFixed,
            HourOfDayPrices = defaultTariffDetail.DefaultHourOfDayPrices
                .Select(h => new HourOfDayPriceState()
                {
                    HourOfDay = h.HourOfDay,
                    PencePerKWh = h.PencePerKWh
                }).ToImmutableList(),
        };
    }

    public static TariffDetail eMapToTariffDto(this TariffDetailState tariffDetailState)
    {
        return new TariffDetail
        {
            DailyStandingChargePence = tariffDetailState.DailyStandingChargePence,
            DateAppliesFrom = tariffDetailState.DateAppliesFrom,
            GlobalId = tariffDetailState.GlobalId,
            HourOfDayPrices = tariffDetailState.HourOfDayPrices.eMapToHourOfDayPriceDto(),
            IsHourOfDayFixed = tariffDetailState.IsHourOfDayFixed,
            PencePerKWh = tariffDetailState.PencePerKWh
        };
    }

    public static TariffDetailState eMapToTariffState(this TariffDetail tariffDetailDto, bool addGuidForNewTariff)
    {
        return new TariffDetailState
        {
            DailyStandingChargePence = tariffDetailDto.DailyStandingChargePence,
            DateAppliesFrom = tariffDetailDto.DateAppliesFrom,
            GlobalId = addGuidForNewTariff ? Guid.NewGuid() : tariffDetailDto.GlobalId,
            HourOfDayPrices = tariffDetailDto.HourOfDayPrices.eMapToHourOfDayPriceState().ToImmutableList(),
            IsHourOfDayFixed = tariffDetailDto.IsHourOfDayFixed,
            PencePerKWh = tariffDetailDto.PencePerKWh
        };
    }

    public static TariffDetailState eCurrentTariffState(this IEnumerable<TariffDetailState> tariffDetails)
    {
        return tariffDetails.Where(c => DateTime.Today >= (c.DateAppliesFrom ?? DateTime.MaxValue))
            .MaxBy(c => c.DateAppliesFrom);
    }

    public static TariffDetail eCurrentTariff(this IEnumerable<TariffDetail> tariffDetails)
    {
        return tariffDetails.Where(c => DateTime.Today >= (c.DateAppliesFrom ?? DateTime.MaxValue))
            .MaxBy(c => c.DateAppliesFrom);
    }

    public static TEnum eStringToEnum<TEnum>(this string value) where TEnum : struct, Enum
    {
        bool result = Enum.TryParse(value, out TEnum enumResult);
        if (result)
        {
            return enumResult;
        }
        throw new ArgumentException($"{value} is not a value name of Enum T");
    }

    public static string eMeterLastUpdateKey(this Meter meter)
    {
        return $"{meter.GlobalId}-lastUpdate";
    }

    public static string eMeterCoefficientsKey(this Meter meter)
    {
        return $"{meter.GlobalId}-coefficients";
    }

    public static string eMeterDataKey(this Meter meter)
    {
        return $"{meter.GlobalId}-meterData";
    }

    public static string eBasicReadingsKey(this Guid globalId)
    {
        return $"{globalId}-basicReadings";
    }

    public static string eMeterCoefficientsKey(this Guid globalId)
    {
        return $"{globalId}-coefficients";
    }

    public static string eMeterDataKey(this Guid globalId)
    {
        return $"{globalId}-meterData";
    }

    public static string eBasicReadingsKey(this Meter meter)
    {
        return $"{meter.GlobalId}-basicReadings";
    }


    public static string eTariffUnitRateText(this TariffDetailState tariffDetailState)
    {

        if (tariffDetailState.IsHourOfDayFixed)
        {
            return $"Unit Rate: {tariffDetailState.PencePerKWh:N0}p/kWh";
        }

        HourOfDayPriceState maxCost = tariffDetailState.HourOfDayPrices.MaxBy(c => c.PencePerKWh);
        HourOfDayPriceState minCost = tariffDetailState.HourOfDayPrices.MinBy(c => c.PencePerKWh);
        return $"Variable Unit Rate: {minCost.PencePerKWh:N0}p/kWh - {maxCost.PencePerKWh:N0}p/kWh";
    }

    public static string eTariffUnitRateText(this TariffDetail tariffDetail)
    {

        if (tariffDetail.IsHourOfDayFixed)
        {
            return $"Unit Rate: {tariffDetail.PencePerKWh:N0}p/kWh";
        }

        HourOfDayPrice maxCost = tariffDetail.HourOfDayPrices.MaxBy(c => c.PencePerKWh);
        HourOfDayPrice minCost = tariffDetail.HourOfDayPrices.MinBy(c => c.PencePerKWh);
        return $"Variable Unit Rate: {minCost.PencePerKWh:N0}p/kWh - {maxCost.PencePerKWh:N0}p/kWh";
    }

    public static string eTemperatureRangeText(this (int low, int high) range)
    {
        return $"{range.low} - {range.high}°C";
    }
    public static string eDateToMinimal(this DateTime date)
    {
        return date.eToString("ddd dnn MMM", useExtendedSpecifiers: true);
    }

    public static bool eUpToDate(this DateTime? latestReading)
    {
        return latestReading.HasValue && latestReading >= DateTime.Today.AddDays(-1).Date;
    }

    public static IEnumerable<DateTime> eGenerateAllDatesBetween(this DateTime startDate, DateTime endDate, bool endDateInclusive = true)
    {
        DateTime currentDate = startDate.Date;
        DateTime lastDateToInclude = endDateInclusive ? endDate.Date : endDate.AddDays(-1).Date;
        while (currentDate <= lastDateToInclude)
        {
            yield return currentDate;
            currentDate = currentDate.AddDays(1);
        }

    }

    public static int eGetDateCountInclusive(this DateTime startDate, DateTime endDate)
    {
        List<DateTime> dates = startDate.Date.eGenerateAllDatesBetween(endDate, endDateInclusive: true).ToList();
        return dates.Count;
    }

    public static int eGetDateCountExcludeEnd(this DateTime startDate, DateTime endDate)
    {
        List<DateTime> dates = startDate.Date.eGenerateAllDatesBetween(endDate, endDateInclusive: false).ToList();
        return dates.Count;
    }

    public static string ePenceRateDisplay(this decimal amount)
    {
        return $"{amount:F2}p";
    }

    public static string eToMoneyFormat(this decimal amount)
    {
        return $"{amount:F2}";
    }

    public static string eReplaceEmptySummary(this string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return "No Data (seasonal estimated temperature)";
        }
        return summary;
    }

    public static string eToLongDateString(this DateTime? nullableDateTime)
    {
        if (!nullableDateTime.HasValue)
        {
            return string.Empty;
        }

        return nullableDateTime.Value.eToLongDateString();
    }

    public static string eToLongDateString(this DateTime dateTime)
    {
        return dateTime.eToString("dddd, dnn MMMM yyyy", true);
    }



    public static string eToString(this DateTime? nullableDateTime, string format, bool useExtendedSpecifiers)
    {
        if (!nullableDateTime.HasValue)
        {
            return string.Empty;
        }

        return nullableDateTime.Value.eToString(format, useExtendedSpecifiers);
    }

    public static string eToString(this DateTime dateTime, string format, bool useExtendedSpecifiers)
    {
        return useExtendedSpecifiers
            ? dateTime.ToString(format)
                .Replace("nn", dateTime.Day.eToOccurrenceSuffix().ToLower())
                .Replace("NN", dateTime.Day.eToOccurrenceSuffix().ToUpper())
            : dateTime.ToString(format);
    }

    public static MeterState eMapToMeterState(this Meter meter)
    {
        return new MeterState
        {
            Authorized = meter.Authorized,
            GlobalId = meter.GlobalId,
            MeterType = meter.MeterType,
            Mpxn = meter.Mpxn,
        };
    }

    public static Meter eMapToMeterDto(this MeterState meter)
    {
        return new Meter
        {
            Authorized = meter.Authorized,
            GlobalId = meter.GlobalId,
            MeterType = meter.MeterType,
            Mpxn = meter.Mpxn,
        };
    }

    public static string eToOccurrenceSuffix(this int integer)
    {
        return (integer % 100) switch
        {
            11 or 12 or 13 => "th",
            _ => (integer % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th",
            },
        };
    }

    public static string eTempToHexColour(this int temp)
    {
        return ((double)temp).eTempToHexColour();
    }

    public static string eTempToHexColour(this double temp)
    {
        return temp switch
        {
            <= -20 => "#0054ff",
            <= -19 => "#0054ff",
            <= -18 => "#0054ff",
            <= -17 => "#0054ff",
            <= -16 => "#0054ff",
            <= -15 => "#0054ff",
            <= -14 => "#0054ff",
            <= -13 => "#0054ff",
            <= -12 => "#0054ff",
            <= -11 => "#0094ff",
            <= -10 => "#0094ff",
            <= -9 => "#0094ff",
            <= -8 => "#0094ff",
            <= -7 => "#00b4ff",
            <= -6 => "#00b4ff",
            <= -5 => "#00b4ff",
            <= -4 => "#00b4ff",
            <= -3 => "#00d4ff",
            <= -2 => "#00d4ff",
            <= -1 => "#00d4ff",
            <= 0 => "#00d4ff",
            <= 1 => "#00ffd0",
            <= 2 => "#00ffd0",
            <= 3 => "#00ffd0",
            <= 4 => "#00ffd0",
            <= 5 => "#d7ff00",
            <= 6 => "#d7ff00",
            <= 7 => "#d7ff00",
            <= 8 => "#d7ff00",
            <= 9 => "#ffe600",
            <= 10 => "#ffe600",
            <= 11 => "#ffe600",
            <= 12 => "#ffe600",
            <= 13 => "#ffd200",
            <= 14 => "#ffd200",
            <= 15 => "#ffd200",
            <= 16 => "#ffd200",
            <= 17 => "#ffb400",
            <= 18 => "#ffb400",
            <= 19 => "#ffb400",
            <= 20 => "#ffb400",
            <= 21 => "#ffa000",
            <= 22 => "#ffa000",
            <= 23 => "#ffa000",
            <= 24 => "#ffa000",
            <= 25 => "#ff7800",
            <= 26 => "#ff7800",
            <= 27 => "#ff7800",
            <= 28 => "#ff7800",
            <= 29 => "#ff5000",
            <= 30 => "#ff5000",
            <= 31 => "#ff5000",
            <= 32 => "#ff5000",
            <= 33 => "#ff0000",
            <= 34 => "#ff0000",
            <= 35 => "#ff0000",
            <= 36 => "#ff0000",
            <= 37 => "#ff0000",
            <= 38 => "#ff0000",
            <= 39 => "#ff0000",
            <= 40 => "#ff0000",
            _ => "#ff0000",
        };
    }

    public static DateTime eStartOfWeek(this DateTime date, DayOfWeek startOfWeek)
    {
        int diff = date.DayOfWeek - startOfWeek;
        if (diff < 0)
        {
            diff += 7;
        }

        return date.AddDays(-1 * diff).Date;
    }



    public static IEnumerable<T> eToIEnumerable<T>(this T[] items)
    {
        return items;
    }
#pragma warning restore IDE1006 // Naming Styles

}
