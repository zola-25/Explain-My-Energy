using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.Weather.Store
{
    [FeatureState(Name = nameof(WeatherState))]
    [PersistState]
    public record WeatherState
    {
        [property: JsonIgnore]
        public bool Loading { get; init; }
        public ImmutableList<DailyWeatherRecord> WeatherReadings { get; init; }


        public DateTime LastUpdated { get; init; }

        public WeatherState()
        {
            Loading = false;
            WeatherReadings = ImmutableList<DailyWeatherRecord>.Empty;
            LastUpdated = DateTime.MinValue;
        }

    }

}
