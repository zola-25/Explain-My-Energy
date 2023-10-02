using Energy.Shared;
using Energy.WeatherReadings.Interfaces;
using Energy.WeatherReadings.Models;
using System.Text.Json;
using Energy.WeatherReadings;

namespace Energy.WeatherReadings;

public class MeteoWeatherApiCalls : IMeteoWeatherApiCalls
{

    private readonly IHttpClientFactory _httpClientFactory;

    public MeteoWeatherApiCalls(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task LoadHistorical(OutCodeWeatherUpdateRanges outCodeUpdateRange, List<DailyWeatherReading> dailyWeatherReadings)
    {
        (DateTime start, DateTime end, bool update) historicalRange = outCodeUpdateRange.GetHistoricalRange();

        if (!historicalRange.update)
        {
            return;
        }

        Dictionary<string, string> queryParameters = new Dictionary<string, string>()
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

        string queryString = queryParameters.BuildQueryString();
        string urlHistorical = $"archive?{queryString}";

        HttpClient httpClientHistorical = _httpClientFactory.CreateClient("Historical");
        HttpResponseMessage response = await httpClientHistorical.GetAsync(urlHistorical);

        ValidateResponse(WeatherReadingType.Historical,
            historicalRange.start,
            historicalRange.end,
            outCodeUpdateRange,
            response,
            urlHistorical);


        string respString = await response.Content.ReadAsStringAsync();


        using (JsonDocument doc = JsonDocument.Parse(respString))
        {
            JsonElement root = doc.RootElement;
            JsonElement dailyJson = root.GetProperty("daily");

            long[] time = dailyJson.GetProperty("time").EnumerateArray().Select(x => x.GetInt64()).ToArray();
            int?[] weathercode = dailyJson.GetProperty("weathercode").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (int?)null : x.GetInt32()).ToArray();
            decimal?[] temperature_2m_mean = dailyJson.GetProperty("temperature_2m_mean").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] apparent_temperature_min = dailyJson.GetProperty("apparent_temperature_min").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] apparent_temperature_max = dailyJson.GetProperty("apparent_temperature_max").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] temperature_2m_min = dailyJson.GetProperty("temperature_2m_min").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] temperature_2m_max = dailyJson.GetProperty("temperature_2m_max").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            long?[] sunrise = dailyJson.GetProperty("sunrise").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (long?)null : x.GetInt64()).ToArray();
            long?[] sunset = dailyJson.GetProperty("sunset").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (long?)null : x.GetInt64()).ToArray();
            decimal?[] windspeed_10m_max = dailyJson.GetProperty("windspeed_10m_max").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] rain_sum = dailyJson.GetProperty("rain_sum").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] snowfall_sum = dailyJson.GetProperty("snowfall_sum").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();


            for (int i = 0; i < time.Length; i++)
            {
                DailyWeatherReading reading = new DailyWeatherReading
                {
                    UtcTime = DateTimeOffset.FromUnixTimeSeconds(time[i]).DateTime,
                    Summary = weathercode[i].WeatherCodeToSummary(),
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
        (DateTime start, DateTime end) forecastRange = outCodeUpdateRange.GetNearTermForecastRange();

        Dictionary<string, string> queryParameters = new Dictionary<string, string>()
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


        string queryString = queryParameters.BuildQueryString();
        string urlForecast = $"forecast?{queryString}";

        HttpClient httpClientForecast = _httpClientFactory.CreateClient("Forecast");

        HttpResponseMessage response = await httpClientForecast.GetAsync(urlForecast);

        ValidateResponse(WeatherReadingType.Forecast,
            forecastRange.start,
            forecastRange.end,
            outCodeUpdateRange,
            response,
            urlForecast);


        string respString = await response.Content.ReadAsStringAsync();
        using (JsonDocument doc = JsonDocument.Parse(respString))
        {
            JsonElement root = doc.RootElement;
            JsonElement dailyJson = root.GetProperty("daily");

            long[] time = dailyJson.GetProperty("time").EnumerateArray().Select(x => x.GetInt64()).ToArray();
            int?[] weathercode = dailyJson.GetProperty("weathercode").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (int?)null : x.GetInt32()).ToArray();
            decimal?[] apparent_temperature_min = dailyJson.GetProperty("apparent_temperature_min").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] apparent_temperature_max = dailyJson.GetProperty("apparent_temperature_max").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] temperature_2m_min = dailyJson.GetProperty("temperature_2m_min").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] temperature_2m_max = dailyJson.GetProperty("temperature_2m_max").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            long?[] sunrise = dailyJson.GetProperty("sunrise").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (long?)null : x.GetInt64()).ToArray();
            long?[] sunset = dailyJson.GetProperty("sunset").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (long?)null : x.GetInt64()).ToArray();
            decimal?[] windspeed_10m_max = dailyJson.GetProperty("windspeed_10m_max").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] rain_sum = dailyJson.GetProperty("rain_sum").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] snowfall_sum = dailyJson.GetProperty("snowfall_sum").EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();

            for (int i = 0; i < time.Length; i++)
            {
                DateTime readDate = DateTimeOffset.FromUnixTimeSeconds(time[i]).DateTime;
                DailyWeatherReading reading = new DailyWeatherReading
                {
                    UtcTime = readDate,
                    Summary = weathercode[i].WeatherCodeToSummary(),
                    ApparentTemperatureMin = apparent_temperature_min[i],
                    ApparentTemperatureMax = apparent_temperature_max[i],
                    TemperatureAverage = Math.Round(((temperature_2m_max[i] ?? 0) + (temperature_2m_min[i] ?? 0)) / 2m, 2),
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
        (DateTime start, DateTime end) climateRange = outCodeUpdateRange.GetLongTermForecastRange();

        Dictionary<string, string> queryParameters = new Dictionary<string, string>()
        {
            { "latitude", outCodeUpdateRange.Latitude.ToString("G") },
            { "longitude", outCodeUpdateRange.Longitude.ToString("G") },
            { "start_date", climateRange.start.ToString("yyyy-MM-dd") },
            { "end_date", climateRange.end.ToString("yyyy-MM-dd") },
            { "daily", "temperature_2m_mean,temperature_2m_max,temperature_2m_min,windspeed_10m_max,rain_sum,snowfall_sum" },
            { "models", "EC_Earth3P_HR" },
            { "timeformat", "unixtime" }
        };

        string queryString = queryParameters.BuildQueryString();
        string urlClimate = $"climate?{queryString}";

        HttpClient httpClientClimate = _httpClientFactory.CreateClient("Climate");

        HttpResponseMessage response = await httpClientClimate.GetAsync(urlClimate);

        ValidateResponse(WeatherReadingType.Climate,
            climateRange.start,
            climateRange.end,
            outCodeUpdateRange,
            response,
            urlClimate);

        string respString = await response.Content.ReadAsStringAsync();
        using (JsonDocument doc = JsonDocument.Parse(respString))
        {

            JsonElement root = doc.RootElement;
            JsonElement dailyJson = root.GetProperty("daily");

            long[] time = dailyJson.GetProperty("time").EnumerateArray().Select(x => x.GetInt64()).ToArray();
            decimal?[] temperature_2m_mean = dailyJson.GetProperty("temperature_2m_mean").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] temperature_2m_min = dailyJson.GetProperty("temperature_2m_min").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] temperature_2m_max = dailyJson.GetProperty("temperature_2m_max").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] windspeed_10m_max = dailyJson.GetProperty("windspeed_10m_max").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] rain_sum = dailyJson.GetProperty("rain_sum").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();
            decimal?[] snowfall_sum = dailyJson.GetProperty("snowfall_sum").EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Null ? (decimal?)null : x.GetDecimal()).ToArray();

            for (int i = 0; i < time.Length; i++)
            {
                DateTime readDate = DateTimeOffset.FromUnixTimeSeconds(time[i]).DateTime;
                DailyWeatherReading reading = new DailyWeatherReading
                {
                    UtcTime = readDate,
                    Summary = "Long-term Climate Forecast",
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

    private static void ValidateResponse(
        WeatherReadingType updateType,
        DateTime startDate,
        DateTime endDate,
        OutCodeWeatherUpdateRanges outcodeUpdateRange,
        HttpResponseMessage response,
        string url)
    {
        if (!response.IsSuccessStatusCode)
        {
            WeatherApiBadResponse badResponse = new WeatherApiBadResponse()
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
