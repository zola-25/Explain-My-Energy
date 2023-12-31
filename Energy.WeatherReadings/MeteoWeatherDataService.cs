﻿using Energy.Shared;
using Energy.WeatherReadings.Interfaces;
using Energy.WeatherReadings.Models;
using Microsoft.Extensions.Logging;

namespace Energy.WeatherReadings;

public class MeteoWeatherDataService : IMeteoWeatherDataService
{
    private readonly IOutCodeLocationLookup _outCodeLocationLookup;
    private readonly IMeteoWeatherApiCalls _weatherApiCalls;
    private readonly ILogger<MeteoWeatherDataService> _logger;


    public MeteoWeatherDataService(IOutCodeLocationLookup outCodeLocationLookup, IMeteoWeatherApiCalls weatherApiCalls, ILogger<MeteoWeatherDataService> logger)
    {
        _outCodeLocationLookup = outCodeLocationLookup;
        _weatherApiCalls = weatherApiCalls;
        _logger = logger;
    }

    public async Task<List<DailyWeatherReading>> GetForOutCode(string outCode, DateTime? latestHistorical = null, DateTime? latestReading = null)
    {
        OutCodeLocation outCodeLocation = await _outCodeLocationLookup.LocationLookup(outCode);

        OutCodeWeatherUpdateRanges weatherUpdateRange = new OutCodeWeatherUpdateRanges()
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
        List<DailyWeatherReading> readingsToUpsert = new List<DailyWeatherReading>();
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