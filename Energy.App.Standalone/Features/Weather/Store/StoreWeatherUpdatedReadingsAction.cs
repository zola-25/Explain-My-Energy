﻿using Energy.Shared;

namespace Energy.App.Standalone.Features.Weather.Store
{
    public class StoreWeatherUpdatedReadingsAction
    {
        public List<DailyWeatherReading> UpdatedWeatherReadings { get; }

        public StoreWeatherUpdatedReadingsAction(List<DailyWeatherReading> updatedWeatherReadings)
        {
            UpdatedWeatherReadings = updatedWeatherReadings;
        }
    }
}