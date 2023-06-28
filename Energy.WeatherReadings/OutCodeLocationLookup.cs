using Energy.Shared;
using Energy.WeatherReadings.Interfaces;
using Energy.WeatherReadings.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Energy.WeatherReadings
{
    public class OutCodeLocationLookup : IOutCodeLocationLookup
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OutCodeLocationLookup> _logger;


        public OutCodeLocationLookup(IHttpClientFactory httpClientFactory, ILogger<OutCodeLocationLookup> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<OutCodeLocation> LocationLookup(string inputOutCode)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("PostcodeLocationLookup");

            string json = await httpClient.GetStringAsync($"outcodes/{inputOutCode.Trim()}");

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;
                JsonElement result = root.GetProperty("result");
                double latitude = result.GetProperty("latitude").GetDouble();
                double longitude = result.GetProperty("longitude").GetDouble();
                string responseOutCode = result.GetProperty("outcode").GetString();

                if (!String.Equals(responseOutCode, inputOutCode, StringComparison.InvariantCultureIgnoreCase))
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

        public async Task<OutCodeValidationResult> OutCodeVerification(string inputOutCode)
        {
            try
            {
                HttpClient httpClient = _httpClientFactory.CreateClient("PostcodeLocationLookup");

                HttpResponseMessage response = await httpClient.GetAsync($"outcodes/{inputOutCode.Trim()}");

                if (response.IsSuccessStatusCode)
                {
                    return new OutCodeValidationResult()
                    {
                        Found = true
                    };
                }

                byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();
                (bool parsed, JsonDocument jDoc) = TryParseAsJson(contentBytes);

                string responseMessage = String.Empty;
                if (parsed && jDoc != null)
                {
                    using (jDoc)
                    {
                        JsonElement root = jDoc.RootElement;
                        bool hasErrorMessage = root.TryGetProperty("error", out JsonElement errorProp);
                        if (hasErrorMessage && errorProp.ValueKind == JsonValueKind.String)
                        {
                            responseMessage = errorProp.GetString();
                        }
                    }
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new OutCodeValidationResult()
                    {
                        Found = false,
                        Error = false,
                        ResponseMessage = responseMessage
                    };
                }

                return new OutCodeValidationResult()
                {
                    Found = false,
                    Error = true,
                    ResponseMessage = responseMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEventIds.OutCodeVerificationException, ex, "Exception thrown verifying {OutCode}", inputOutCode);
                return new OutCodeValidationResult()
                {
                    Error = true,
                    Found = false,
                };
            }
        }

        private static (bool parsed, JsonDocument jDoc) TryParseAsJson(byte[] contentBytes)
        {

            ReadOnlySpan<byte> contentSpan = new ReadOnlySpan<byte>(contentBytes);

            Utf8JsonReader utf8JsonReader = new Utf8JsonReader(contentSpan);

            bool parsed = System.Text.Json.JsonDocument.TryParseValue(ref utf8JsonReader, out JsonDocument jDoc);
            return (parsed, jDoc);

        }
    }

    public record OutCodeValidationResult
    {
        public bool Found { get; init; }
        public bool Error { get; init; }
        public string ResponseMessage { get; init; }
    }
}
