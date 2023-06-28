using System.Text;

namespace Energy.WeatherReadings
{
    public static class Extensions
    {
        public static string BuildQueryString(this Dictionary<string, string> queryParameters)
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

    }
}
