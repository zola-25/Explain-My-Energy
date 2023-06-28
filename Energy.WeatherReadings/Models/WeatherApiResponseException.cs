namespace Energy.WeatherReadings.Models;

public class WeatherApiResponseException : Exception
{
    public WeatherApiBadResponse Response { get; }

    public WeatherApiResponseException(WeatherApiBadResponse response)
    {
        Response = response;
    }
}