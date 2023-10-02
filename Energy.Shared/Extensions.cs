using System.Collections.Immutable;
using System.Text;

namespace Energy.Shared;

#pragma warning disable IDE1006 // my extension method prefix convention is 'e'
public static class Extensions
{
    public static string eBuildQueryString(this Dictionary<string, string> queryParameters)
    {
        StringBuilder queryStringBuilder = new StringBuilder();

        foreach (KeyValuePair<string, string> parameter in queryParameters)
        {
            if (queryStringBuilder.Length > 0)
            {
                queryStringBuilder.Append('&');
            }

            string encodedKey = Uri.EscapeDataString(parameter.Key);
            string encodedValue = Uri.EscapeDataString(parameter.Value);

            queryStringBuilder.AppendFormat("{0}={1}", encodedKey, encodedValue);
        }

        return queryStringBuilder.ToString();
    }
    public static List<T> eItemToList<T>(this T item)
    {
        return new List<T> { item };
    }

    public static ImmutableList<T> eItemToImmutableList<T>(this T item)
    {
        return new List<T> { item }.ToImmutableList();
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
}
#pragma warning restore IDE1006 // my extension method prefix convention is 'e'

