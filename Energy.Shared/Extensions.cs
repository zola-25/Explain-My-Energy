using System.Text;

namespace Energy.Shared
{
    public static class Extensions
    {
        public static string BuildQueryString(this Dictionary<string, string> queryParameters)
        {
            var queryStringBuilder = new StringBuilder();

            foreach (var parameter in queryParameters)
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
