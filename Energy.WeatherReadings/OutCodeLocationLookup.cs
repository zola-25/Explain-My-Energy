using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Energy.WeatherReadings.Interfaces;
using Energy.WeatherReadings.Models;

namespace Energy.WeatherReadings
{
    public class OutCodeLocationLookup : IOutCodeLocationLookup
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OutCodeLocationLookup(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<OutCodeLocation> LocationLookup(string inputOutCode)
        {
            var httpClient = _httpClientFactory.CreateClient("PostcodeLocationLookup");

            var json = await httpClient.GetStringAsync($"outcodes/{inputOutCode.Trim()}");

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;
                JsonElement result = root.GetProperty("result");
                double latitude = result.GetProperty("latitude").GetDouble();
                double longitude = result.GetProperty("longitude").GetDouble();
                string responseOutCode = result.GetProperty("outcode").GetString();

                if (responseOutCode != inputOutCode)
                {
                    throw new ApplicationException(
                        "Response outcode from location lookup does not match request outcode");
                }

                return new OutCodeLocation()
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    OutCode = inputOutCode
                };
            }
        }
    }
}
