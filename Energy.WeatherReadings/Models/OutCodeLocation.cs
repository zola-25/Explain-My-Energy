namespace Energy.WeatherReadings.Models;

public record OutCodeLocation
{
    public string OutCode { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}