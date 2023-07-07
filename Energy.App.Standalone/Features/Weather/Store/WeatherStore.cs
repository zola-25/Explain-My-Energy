using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.Weather.Store
{
    [PersistState]
    public record WeatherState
    {
        [property: JsonIgnore]
        public bool Loading { get; init; }
        public ImmutableList<DailyWeatherReading> WeatherReadings { get; init; }

        [property: JsonIgnore]
        public bool Updating { get; init; }
    }

    public class WeatherFeature : Feature<WeatherState>
    {
        public override string GetName()
        {
            return nameof(WeatherFeature);
        }

        protected override WeatherState GetInitialState()
        {
            return new WeatherState
            {
                Loading = false,
                Updating = false,
                WeatherReadings = ImmutableList<DailyWeatherReading>.Empty
            };
        }
    }
}
