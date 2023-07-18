using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast
{
    [FeatureState(Name = nameof(HistoricalForecastState))]
    [PersistState]
    public record HistoricalForecastState
    {
        [property: JsonIgnore]
        public ImmutableList<DailyCostedReading> this[MeterType meterType]
        {
            get
            {
                return meterType switch
                {
                    MeterType.Electricity => ElectricityForecastDailyCosts,
                    MeterType.Gas => GasForecastDailyCosts,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(meterType)),
                };
            } 
        }
        public ImmutableList<DailyCostedReading> GasForecastDailyCosts { get; init; }
        public ImmutableList<DailyCostedReading> ElectricityForecastDailyCosts { get; init; }
        public DateTime ElectricityLastUpdate { get; init; }
        public DateTime GasLastUpdate { get; init; }

        public HistoricalForecastState()
        {
            GasForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty;
            ElectricityForecastDailyCosts = ImmutableList<DailyCostedReading>.Empty;
            ElectricityLastUpdate = DateTime.MinValue;
            GasLastUpdate = DateTime.MinValue;
        }
    }
}
