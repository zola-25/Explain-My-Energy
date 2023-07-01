namespace Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;

public interface IWeatherDataApi
{
    Task<List<DailyWeatherReading>> GetWeatherData(DateTime? from = null);
}