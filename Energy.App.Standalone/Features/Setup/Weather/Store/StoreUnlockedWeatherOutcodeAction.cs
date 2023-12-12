using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Weather.Store;

public class StoreUnlockedWeatherOutcodeAction
{
    public string OutCode { get; init; }

    [ReducerMethod]
    public static WeatherState OnStoreUnlockedWeatherOutcodeReducer(WeatherState state, StoreUnlockedWeatherOutcodeAction action)
    {
        return state with
        {
            OutCode = action.OutCode,
        };
    }
}
