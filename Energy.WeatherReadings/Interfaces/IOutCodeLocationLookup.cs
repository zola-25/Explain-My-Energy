using Energy.WeatherReadings.Models;

namespace Energy.WeatherReadings.Interfaces;

public interface IOutCodeLocationLookup
{
    Task<OutCodeLocation> LocationLookup(string inputOutCode);
    Task<OutCodeValidationResult> OutCodeVerification(string inputOutCode);
}