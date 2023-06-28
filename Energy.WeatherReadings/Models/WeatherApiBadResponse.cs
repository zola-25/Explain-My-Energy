using Energy.Shared;

namespace Energy.WeatherReadings.Models;

public class WeatherApiBadResponse
{
    public string Url { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime StartDate { get; set; }

    public string OutCodeCharacters { get; set; }
    public string ReasonPhrase { get; set; }
    public int StatusCode { get; set; }
    public WeatherReadingType UpdateType { get; set; }

    public override string ToString()
    {
        return $"WeatherApi API responded with {StatusCode} for {Url} \r\nReason: {ReasonPhrase}";
    }
}