using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

public interface IForecastCoefficientsCreator
{
    (decimal C, decimal Gradient) GetForecastCoefficients(ImmutableList<BasicReading> basicReadings, ImmutableList<DailyWeatherReading> dailyWeatherReadings);
}