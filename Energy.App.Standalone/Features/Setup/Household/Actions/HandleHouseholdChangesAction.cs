using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Setup.Meter.Store;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;
using Fluxor;

namespace Energy.App.Standalone.Features.Setup.Household.Actions;

public class HandleHouseholdChangesAction
{
    public bool UpdateHeatingForecast { get; }
    public bool ResetHeatingForecast { get; }
    public MeterType PrimaryHeatSource { get; set; }
    public bool UpdateWeatherData { get; }

    public HandleHouseholdChangesAction(bool updateWeatherData,
                                        bool updateHeatingForecast,
                                        bool resetHeatingForecast,
                                        MeterType primaryHeatSource)
    {
        UpdateHeatingForecast = updateHeatingForecast;
        ResetHeatingForecast = resetHeatingForecast;
        UpdateWeatherData = updateWeatherData;
        PrimaryHeatSource = primaryHeatSource;
    }



    private class HandleHouseholdChangesEffect : Effect<HandleHouseholdChangesAction>
    {
        public override async Task HandleAsync(HandleHouseholdChangesAction action, IDispatcher dispatcher)
        {

            if (action.UpdateWeatherData)
            {
                var weatherCompletion = new TaskCompletionSource<(bool, string)>();
                dispatcher.Dispatch(new EnsureWeatherLoadedAction(forceReload: true, weatherCompletion));
                await weatherCompletion.Task;
            }

            if (action.ResetHeatingForecast)
            {
                dispatcher.Dispatch(new DeleteHeatingForecastAction(action.PrimaryHeatSource));
            }
            else if (action.UpdateWeatherData || action.UpdateHeatingForecast)
            {
                var heatingMeterCompletion = new TaskCompletionSource<(bool, string)>();
                dispatcher.Dispatch(new EnsureHeatingSetupAction(forceReloadCoefficients: true, forceReloadHeatingForecast: true, completion: heatingMeterCompletion));
                await heatingMeterCompletion.Task;
            }
        }
    }
}
