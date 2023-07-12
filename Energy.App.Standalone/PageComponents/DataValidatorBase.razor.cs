using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Fluxor.Persist.Middleware;
using Microsoft.AspNetCore.Components;

namespace Energy.App.Standalone.PageComponents
{
    public partial class DataValidatorBase : FluxorComponent
    {
        [Inject]
        IDispatcher Dispatcher { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        public bool WeatherReady { get; private set; }

        public bool GasReadingsReady { get; private set; }

        public bool AppLoading = true;

        public TaskCompletionSource<bool> StateLoadedFromStorage;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override async Task OnInitializedAsync()
        {
            this.eLogToConsole(nameof(OnInitializedAsync));

            StateLoadedFromStorage = new TaskCompletionSource<bool>();

            SubscribeToAction<InitializePersistMiddlewareResultSuccessAction>(result =>
            {
                Console.WriteLine($"**** State rehydrated ****");
                StateLoadedFromStorage.SetResult(true);
            });

            await StateLoadedFromStorage.Task;

            var weatherCompletion = new TaskCompletionSource<int>();
            Dispatcher.Dispatch(new EnsureWeatherLoadedAction(false, weatherCompletion));

            int numWeatherDaysUpdated = await weatherCompletion.Task;

            WeatherReady = true;


            var gasReadingsCompletion = new TaskCompletionSource<int>();
            Dispatcher.Dispatch(new EnsureGasReadingsLoadedAction(false, gasReadingsCompletion));

            int numGasReadingsUpdated = await gasReadingsCompletion.Task;
            // dispatch analysis action
            GasReadingsReady = true;

            var forecastCompletion = new TaskCompletionSource<bool>();
            Dispatcher.Dispatch(new UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction())

            AppLoading = false;
        }



    }
}
