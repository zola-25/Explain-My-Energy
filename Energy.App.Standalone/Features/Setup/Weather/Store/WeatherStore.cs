using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Setup.Weather.Store
{
    [PersistState]
    public record WeatherState
    {
        [property: JsonIgnore]
        public bool Loading { get; init; }
        public ImmutableList<DailyWeatherReading> WeatherReadings { get; init; }


        public DateTime LastUpdated { get; init; }

        
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
                LastUpdated = DateTime.MinValue,
                WeatherReadings = ImmutableList<DailyWeatherReading>.Empty
            };
        }
    }
}
