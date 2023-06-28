using Energy.WeatherReadings.Interfaces;
using Energy.WeatherReadings.Models;
using System.Text.Json;
using Energy.Shared;

namespace Energy.WeatherReadings
{
    public class WeatherApiCalls : IWeatherApiCalls
    {

        private readonly IHttpClientFactory _httpClientFactory;

        public WeatherApiCalls(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task LoadHistorical(OutCodeWeatherUpdateRanges outCodeUpdateRange, List<DailyWeatherReading> dailyWeatherReadings)
        {
            var historicalRange = outCodeUpdateRange.GetHistoricalRange();

            if (!historicalRange.update)
            {
                return;
            }

            var queryParameters = new Dictionary<string, string>()
            {
                { "latitude", outCodeUpdateRange.Latitude.ToString("G") },
                { "longitude", outCodeUpdateRange.Longitude.ToString("G") },
                { "start_date", historicalRange.start.ToString("yyyy-MM-dd") },
                { "end_date", historicalRange.end.ToString("yyyy-MM-dd") },
                { "models", "best_match" },
                {
                    "daily",
                    "weathercode,temperature_2m_max,temperature_2m_min,temperature_2m_mean,apparent_temperature_max,apparent_temperature_min,apparent_temperature_mean,sunrise,sunset,windspeed_10m_max,rain_sum,snowfall_sum"
                },
                { "timezone", "GMT" },
                { "timeformat", "unixtime" }
            };

            var queryString = queryParameters.BuildQueryString();
            var urlHistorical = $"archive?{queryString}";

            var httpClientHistorical = _httpClientFactory.CreateClient("Historical");
            var response = await httpClientHistorical.GetAsync(urlHistorical);

            ValidateResponse(WeatherReadingType.Historical,
                historicalRange.start,
                historicalRange.end,
                outCodeUpdateRange,
                response,
                urlHistorical);


            var respString = await response.Content.ReadAsStringAsync();


            using (var doc = JsonDocument.Parse(respString))
            {
                JsonElement root = doc.RootElement;
                JsonElement dailyJson = root.GetProperty("daily");

                long[] time = dailyJson.GetProperty("time").EnumerateArray().Select(x => x.GetInt64()).ToArray();
                int?[] weathercode = dailyJson.GetProperty("weathercode").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (int?)null : x.GetInt32()).ToArray();
                double?[] temperature_2m_mean = dailyJson.GetProperty("temperature_2m_mean").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] apparent_temperature_min = dailyJson.GetProperty("apparent_temperature_min").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] apparent_temperature_max = dailyJson.GetProperty("apparent_temperature_max").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] temperature_2m_min = dailyJson.GetProperty("temperature_2m_min").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] temperature_2m_max = dailyJson.GetProperty("temperature_2m_max").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                long?[] sunrise = dailyJson.GetProperty("sunrise").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (long?)null : x.GetInt64()).ToArray();
                long?[] sunset = dailyJson.GetProperty("sunset").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (long?)null : x.GetInt64()).ToArray();
                double?[] windspeed_10m_max = dailyJson.GetProperty("windspeed_10m_max").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] rain_sum = dailyJson.GetProperty("rain_sum").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] snowfall_sum = dailyJson.GetProperty("snowfall_sum").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();


                for (int i = 0; i < time.Length; i++)
                {
                    var reading = new DailyWeatherReading
                    {
                        ReadDate = DateTimeOffset.FromUnixTimeSeconds(time[i]).DateTime,
                        Icon = weathercode[i].HasValue ? weathercode[i].Value.ToString() : null,
                        TemperatureAverage = temperature_2m_mean[i] ?? 0,
                        ApparentTemperatureMin = apparent_temperature_min[i],
                        ApparentTemperatureMax = apparent_temperature_max[i],
                        TemperatureMin = temperature_2m_min[i],
                        TemperatureMax = temperature_2m_max[i],
                        Sunrise = sunrise[i].HasValue ? DateTimeOffset.FromUnixTimeSeconds(sunrise[i].Value).DateTime : null,
                        Sunset = sunset[i].HasValue ? DateTimeOffset.FromUnixTimeSeconds(sunset[i].Value).DateTime : null,
                        WindSpeed = windspeed_10m_max[i],
                        TotalRain = rain_sum[i],
                        TotalSnowfall = snowfall_sum[i],
                        OutCode = outCodeUpdateRange.OutCode,
                        IsClimateForecast = false,
                        IsHistorical = true,
                        IsNearForecast = false,
                        IsRecentForecast = false,
                        Updated = DateTime.UtcNow
                    };
                    dailyWeatherReadings.Add(reading);
                }
            }

        }

        public async Task LoadForecast(OutCodeWeatherUpdateRanges outCodeUpdateRange, List<DailyWeatherReading> dailyWeatherReadings)
        {
            var forecastRange = outCodeUpdateRange.GetNearTermForecastRange();

            var queryParameters = new Dictionary<string, string>()
            {
                { "latitude", outCodeUpdateRange.Latitude.ToString("G") },
                { "longitude", outCodeUpdateRange.Longitude.ToString("G") },
                { "models", "best_match" },
                { "daily", "weathercode,temperature_2m_max,temperature_2m_min,apparent_temperature_max,apparent_temperature_min,sunrise,sunset,rain_sum,snowfall_sum,windspeed_10m_max" },
                { "timeformat", "unixtime" },
                { "start_date", forecastRange.start.ToString("yyyy-MM-dd") },
                { "end_date", forecastRange.end.ToString("yyyy-MM-dd") },
                { "timezone", "GMT" }
            };


            var queryString = queryParameters.BuildQueryString();
            var urlForecast = $"forecast?{queryString}";

            var httpClientForecast = _httpClientFactory.CreateClient("Forecast");

            var response = await httpClientForecast.GetAsync(urlForecast);

            ValidateResponse(WeatherReadingType.Forecast,
                forecastRange.start,
                forecastRange.end,
                outCodeUpdateRange,
                response,
                urlForecast);


            var respString = await response.Content.ReadAsStringAsync();
            using (var doc = JsonDocument.Parse(respString))
            {
                JsonElement root = doc.RootElement;
                JsonElement dailyJson = root.GetProperty("daily");

                long[] time = dailyJson.GetProperty("time").EnumerateArray().Select(x => x.GetInt64()).ToArray();
                int?[] weathercode = dailyJson.GetProperty("weathercode").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (int?)null : x.GetInt32()).ToArray();
                double?[] apparent_temperature_min = dailyJson.GetProperty("apparent_temperature_min").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] apparent_temperature_max = dailyJson.GetProperty("apparent_temperature_max").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] temperature_2m_min = dailyJson.GetProperty("temperature_2m_min").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] temperature_2m_max = dailyJson.GetProperty("temperature_2m_max").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                long?[] sunrise = dailyJson.GetProperty("sunrise").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (long?)null : x.GetInt64()).ToArray();
                long?[] sunset = dailyJson.GetProperty("sunset").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (long?)null : x.GetInt64()).ToArray();
                double?[] windspeed_10m_max = dailyJson.GetProperty("windspeed_10m_max").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] rain_sum = dailyJson.GetProperty("rain_sum").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] snowfall_sum = dailyJson.GetProperty("snowfall_sum").EnumerateArray()
                    .Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();

                for (int i = 0; i < time.Length; i++)
                {
                    var readDate = DateTimeOffset.FromUnixTimeSeconds(time[i]).DateTime;
                    var reading = new DailyWeatherReading
                    {
                        ReadDate = readDate,
                        Icon = weathercode[i].HasValue ? weathercode[i].Value.ToString() : null,
                        TemperatureAverage = (temperature_2m_max[i] ?? 0 + temperature_2m_min[i] ?? 0) / 2,
                        ApparentTemperatureMin = apparent_temperature_min[i],
                        ApparentTemperatureMax = apparent_temperature_max[i],
                        TemperatureMin = temperature_2m_min[i],
                        TemperatureMax = temperature_2m_max[i],
                        Sunrise = sunrise[i].HasValue
                            ? DateTimeOffset.FromUnixTimeSeconds(sunrise[i].Value).DateTime
                            : null,
                        Sunset = sunset[i].HasValue
                            ? DateTimeOffset.FromUnixTimeSeconds(sunset[i].Value).DateTime
                            : null,
                        WindSpeed = windspeed_10m_max[i],
                        TotalRain = rain_sum[i],
                        TotalSnowfall = snowfall_sum[i],
                        OutCode = outCodeUpdateRange.OutCode,
                        IsClimateForecast = false,
                        IsHistorical = false,
                        IsNearForecast = readDate >= DateTime.UtcNow,
                        IsRecentForecast = readDate < DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    };
                    dailyWeatherReadings.Add(reading);
                }
            }
        }


        public async Task LoadClimate(OutCodeWeatherUpdateRanges outCodeUpdateRange, List<DailyWeatherReading> dailyWeatherReadings)
        {
            var climateRange = outCodeUpdateRange.GetLongTermForecastRange();

            var queryParameters = new Dictionary<string, string>()
            {
                { "latitude", outCodeUpdateRange.Latitude.ToString("G") },
                { "longitude", outCodeUpdateRange.Longitude.ToString("G") },
                { "start_date", climateRange.start.ToString("yyyy-MM-dd") },
                { "end_date", climateRange.end.ToString("yyyy-MM-dd") },
                { "daily", "temperature_2m_mean,temperature_2m_max,temperature_2m_min,windspeed_10m_max,rain_sum,snowfall_sum" },
                { "models", "EC_Earth3P_HR" },
                { "timeformat", "unixtime" }
            };

            var queryString = queryParameters.BuildQueryString();
            var urlClimate = $"climate?{queryString}";

            var httpClientClimate = _httpClientFactory.CreateClient("Climate");

            var response = await httpClientClimate.GetAsync(urlClimate);

            ValidateResponse(WeatherReadingType.Climate,
                climateRange.start,
                climateRange.end,
                outCodeUpdateRange,
                response,
                urlClimate);

            var respString = await response.Content.ReadAsStringAsync();
            using (JsonDocument doc = JsonDocument.Parse(respString))
            {

                JsonElement root = doc.RootElement;
                JsonElement dailyJson = root.GetProperty("daily");

                long[] time = dailyJson.GetProperty("time").EnumerateArray().Select(x => x.GetInt64()).ToArray();
                double?[] temperature_2m_mean = dailyJson.GetProperty("temperature_2m_mean").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] temperature_2m_min = dailyJson.GetProperty("temperature_2m_min").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] temperature_2m_max = dailyJson.GetProperty("temperature_2m_max").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] windspeed_10m_max = dailyJson.GetProperty("windspeed_10m_max").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] rain_sum = dailyJson.GetProperty("rain_sum").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();
                double?[] snowfall_sum = dailyJson.GetProperty("snowfall_sum").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (double?)null : x.GetDouble()).ToArray();

                for (int i = 0; i < time.Length; i++)
                {
                    var readDate = DateTimeOffset.FromUnixTimeSeconds(time[i]).DateTime;
                    var reading = new DailyWeatherReading
                    {
                        ReadDate = readDate,
                        TemperatureAverage = temperature_2m_mean[i] ?? 0,
                        TemperatureMin = temperature_2m_min[i],
                        TemperatureMax = temperature_2m_max[i],
                        WindSpeed = windspeed_10m_max[i],
                        TotalRain = rain_sum[i],
                        TotalSnowfall = snowfall_sum[i],
                        Updated = DateTime.UtcNow,
                        IsNearForecast = false,
                        IsRecentForecast = false,
                        IsClimateForecast = true,
                        IsHistorical = false,
                        OutCode = outCodeUpdateRange.OutCode
                    };
                    dailyWeatherReadings.Add(reading);
                }
            }
        }

        private void ValidateResponse(
            WeatherReadingType updateType,
            DateTime startDate,
            DateTime endDate,
            OutCodeWeatherUpdateRanges outcodeUpdateRange,
            HttpResponseMessage response,
            string url)
        {
            if (!response.IsSuccessStatusCode)
            {
                var badResponse = new WeatherApiBadResponse()
                {
                    UpdateType = updateType,
                    StatusCode = (int)response.StatusCode,
                    StartDate = startDate,
                    EndDate = endDate,
                    OutCodeCharacters = outcodeUpdateRange.OutCode,
                    ReasonPhrase = response.ReasonPhrase,
                    Url = url
                };

                throw new WeatherApiResponseException(badResponse);
            }
        }

    }
}
