using System.Collections.Immutable;
using System.Text;

namespace Energy.Shared
{
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
    }


}
