﻿using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public interface IForecastGenerator
    {
        List<BasicReading> GetBasicReadingsForecast(decimal degreeDifference, HeatingForecastState linearCoefficientsState, List<DailyWeatherReading> forecastWeatherReadings);
    }
}