using Energy.App.Standalone.Features.Setup.Weather.Store;

namespace Energy.App.Standalone.Data.Weather
{
    public interface IWeatherDataWorkerService
    {
        Task<List<DailyWeatherRecord>> GetForOutCode(string outCode, DateTime? latestHistorical = null, DateTime? latestReading = null);
    }
}