using Energy.Shared;
using Energy.WeatherReadings.Interfaces;
using Energy.WeatherReadings.Models;
using Microsoft.Extensions.Logging;

namespace Energy.WeatherReadings;

public class WeatherDataService : IWeatherDataService
{
    private readonly IOutCodeLocationLookup _outCodeLocationLookup;
    private readonly IWeatherApiCalls _weatherApiCalls;
    private readonly ILogger<WeatherDataService> _logger;


    public WeatherDataService(IOutCodeLocationLookup outCodeLocationLookup, IWeatherApiCalls weatherApiCalls, ILogger<WeatherDataService> logger)
    {
        _outCodeLocationLookup = outCodeLocationLookup;
        _weatherApiCalls = weatherApiCalls;
        _logger = logger;
    }

    public async Task<List<DailyWeatherReading>> GetForOutCode(string outCode, DateTime? latestHistorical = null, DateTime? latestReading = null)
    {
        var outCodeLocation = await _outCodeLocationLookup.LocationLookup(outCode);

        var weatherUpdateRange = new OutCodeWeatherUpdateRanges()
        {
            LatestHistoricalUtc = latestHistorical,
            LatestReadingUtc = latestReading,
            Longitude = outCodeLocation.Longitude,
            Latitude = outCodeLocation.Latitude,
            OutCode = outCodeLocation.OutCode,
            UtcToday = DateTime.UtcNow.Date
        };

        return await GetWeatherReadingsFromMeteo(weatherUpdateRange);

    }

    private async Task<List<DailyWeatherReading>> GetWeatherReadingsFromMeteo(OutCodeWeatherUpdateRanges outcodeUpdateRange)
    {
        var readingsToUpsert = new List<DailyWeatherReading>();
        try
        {
            await _weatherApiCalls.LoadHistorical(outcodeUpdateRange, readingsToUpsert);
        } 
        catch (WeatherApiResponseException badResponseException)
        {
            LogBadResponse(badResponseException);
        }
        catch (Exception ex)
        {
            LogGeneralException(ex, WeatherReadingType.Historical);
        }

        try
        {
            await _weatherApiCalls.LoadForecast(outcodeUpdateRange, readingsToUpsert);
        } 
        catch (WeatherApiResponseException badResponseException)
        {
            LogBadResponse(badResponseException);
        }
        catch (Exception ex)
        {
            LogGeneralException(ex, WeatherReadingType.Forecast);
        }


        try
        {
            await _weatherApiCalls.LoadClimate(outcodeUpdateRange, readingsToUpsert);
        } 
        catch (WeatherApiResponseException badResponseException)
        {
            LogBadResponse(badResponseException);
        }
        catch (Exception ex)
        {
            LogGeneralException(ex, WeatherReadingType.Climate);
        }

        return readingsToUpsert;
    }

    private void LogGeneralException(Exception ex, WeatherReadingType updateType)
    {
        _logger.LogError(LogEventIds.WeatherImportException,
            ex,
            "Exception caught when importing weather data for {UpdateType}",
            updateType
        );
    }

    private void LogBadResponse(WeatherApiResponseException badResponseException)
    {
        _logger.LogError(LogEventIds.WeatherImportBadResponse,
            "Open-Meteo API responded with {StatusCode} for {Url}, Reason: {ReasonPhrase}, Details: {@Error}",
            badResponseException.Response.StatusCode,
            badResponseException.Response.Url,
            badResponseException.Response.ReasonPhrase,
            badResponseException);
    }

}