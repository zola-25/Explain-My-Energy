using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.App.Standalone.Models;
using Energy.App.Standalone.Models.Tariffs;
using Energy.Shared;
using System.Text;

namespace Energy.App.Standalone.Extensions;

public static class Extensions
{


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
            HourOfDayPrices = tariffDetailDto.HourOfDayPrices.eMapToHourOfDayPriceState(),
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

    public static IEnumerable<DateTime> eGenerateAllDatesBetween(this DateTime startDate, DateTime endDate)
    {
        DateTime currentDate = startDate.Date;
        while (currentDate <= endDate.Date)
        {
            yield return currentDate;
            currentDate = currentDate.AddDays(1);
        }

    }

    public static string ePenceRateDisplay(this double amount)
    {
        return $"{amount:F2}p";
    }

    public static string eToMoneyFormat(this double amount)
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
        switch (integer % 100)
        {
            case 11:
            case 12:
            case 13:
                return "th";
        }
        switch (integer % 10)
        {
            case 1:
                return "st";
            case 2:
                return "nd";
            case 3:
                return "rd";
            default:
                return "th";
        }
    }

    public static string eTempToHexColour(this int temp)
    {
        return ((double)temp).eTempToHexColour();
    }

    public static string eTempToHexColour(this double temp)
    {
        switch (temp)
        {
            case <= -20:
                return "#0054ff";
            case <= -19:
                return "#0054ff";
            case <= -18:
                return "#0054ff";
            case <= -17:
                return "#0054ff";
            case <= -16:
                return "#0054ff";
            case <= -15:
                return "#0054ff";
            case <= -14:
                return "#0054ff";
            case <= -13:
                return "#0054ff";
            case <= -12:
                return "#0054ff";
            case <= -11:
                return "#0094ff";
            case <= -10:
                return "#0094ff";
            case <= -9:
                return "#0094ff";
            case <= -8:
                return "#0094ff";
            case <= -7:
                return "#00b4ff";
            case <= -6:
                return "#00b4ff";
            case <= -5:
                return "#00b4ff";
            case <= -4:
                return "#00b4ff";
            case <= -3:
                return "#00d4ff";
            case <= -2:
                return "#00d4ff";
            case <= -1:
                return "#00d4ff";
            case <= 0:
                return "#00d4ff";
            case <= 1:
                return "#00ffd0";
            case <= 2:
                return "#00ffd0";
            case <= 3:
                return "#00ffd0";
            case <= 4:
                return "#00ffd0";
            case <= 5:
                return "#d7ff00";
            case <= 6:
                return "#d7ff00";
            case <= 7:
                return "#d7ff00";
            case <= 8:
                return "#d7ff00";
            case <= 9:
                return "#ffe600";
            case <= 10:
                return "#ffe600";
            case <= 11:
                return "#ffe600";
            case <= 12:
                return "#ffe600";
            case <= 13:
                return "#ffd200";
            case <= 14:
                return "#ffd200";
            case <= 15:
                return "#ffd200";
            case <= 16:
                return "#ffd200";
            case <= 17:
                return "#ffb400";
            case <= 18:
                return "#ffb400";
            case <= 19:
                return "#ffb400";
            case <= 20:
                return "#ffb400";
            case <= 21:
                return "#ffa000";
            case <= 22:
                return "#ffa000";
            case <= 23:
                return "#ffa000";
            case <= 24:
                return "#ffa000";
            case <= 25:
                return "#ff7800";
            case <= 26:
                return "#ff7800";
            case <= 27:
                return "#ff7800";
            case <= 28:
                return "#ff7800";
            case <= 29:
                return "#ff5000";
            case <= 30:
                return "#ff5000";
            case <= 31:
                return "#ff5000";
            case <= 32:
                return "#ff5000";
            case <= 33:
                return "#ff0000";
            case <= 34:
                return "#ff0000";
            case <= 35:
                return "#ff0000";
            case <= 36:
                return "#ff0000";
            case <= 37:
                return "#ff0000";
            case <= 38:
                return "#ff0000";
            case <= 39:
                return "#ff0000";
            case <= 40:
                return "#ff0000";
            default:
                return "#ff0000";
        }
    }


    /// <summary>
    /// Converts an enum value from 'PascalCase' to friendlier 'Pascal case'. Will not work for multi-set flags enums
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="anEnum"></param>
    /// <returns></returns>
    public static string eEnumToFormatted<TEnum>(this TEnum anEnum, bool keepFirstUpper = true, bool keepRestUpper = true) where TEnum : Enum
    {
        int selectedValue = Convert.ToInt32(anEnum);

        string selectedName = Enum.GetName(typeof(TEnum), selectedValue);

        return selectedName.eFormatPascalCaseToReadable(keepFirstUpper, keepRestUpper);

    }


    public static string eFormatPascalCaseToReadable(this string pascalCaseString, bool keepFirstUpper = true, bool keepRestUpper = false)
    {
        char[] split = pascalCaseString.ToCharArray();
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < split.Length; i++)
        {
            char character = split[i];

            if (i == 0 && keepFirstUpper)
            {
                char upperChar = char.ToUpperInvariant(character);
                sb.Append(upperChar);
                continue;
            }

            if (char.IsUpper(character) && !char.IsDigit(character))
            {
                sb.Append(' ');
                sb.Append(keepRestUpper ? character : char.ToLowerInvariant(character));
                continue;
            }

            if (char.IsDigit(character) && !char.IsDigit(split[i - 1]))
            {
                sb.Append(' ');
                sb.Append(character);
                continue;
            }

            sb.Append(character);

        }
        string formattedName = sb.ToString();
        return formattedName;
    }

    public static IEnumerable<T> eToIEnumerable<T>(this T[] items)
    {
        return items;
    }

}
