using Energy.App.Standalone.Services;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Weather.Store;

public class StoreLockedWeatherOutcodeAction
{
    public string OutCode { get; init; }
    public bool OutCodeLocked { get; init; }

    [ReducerMethod]
    public static WeatherState OnStoreLockedWeatherOutcodeReducer(WeatherState state, StoreLockedWeatherOutcodeAction action)
    {
        var outCode = action.OutCodeLocked ? action.OutCode : state.OutCode;
        return state with
        {
            OutCode = outCode,
            OutCodeLocked = action.OutCodeLocked
        };
    }
}
