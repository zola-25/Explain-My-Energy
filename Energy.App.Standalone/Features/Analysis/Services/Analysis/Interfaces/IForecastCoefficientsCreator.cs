using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

public interface IForecastCoefficientsCreator
{
    (decimal C, decimal Gradient) GetForecastCoefficients(
        IEnumerable<BasicReading> basicReadings, IEnumerable<DailyWeatherRecord> dailyWeatherReadings);
}