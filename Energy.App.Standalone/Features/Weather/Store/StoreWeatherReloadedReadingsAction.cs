﻿using Energy.Shared;

namespace Energy.App.Standalone.Features.Weather.Store
{
    public class StoreWeatherReloadedReadingsAction
    {
        public List<DailyWeatherReading> ReloadedWeatherReadings { get; }

        public StoreWeatherReloadedReadingsAction(List<DailyWeatherReading> reloadedWeatherReadings)
        {
            ReloadedWeatherReadings = reloadedWeatherReadings;
        }
    }
}