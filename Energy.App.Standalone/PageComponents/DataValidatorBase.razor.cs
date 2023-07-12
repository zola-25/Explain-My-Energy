using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Fluxor.Persist.Middleware;
using Microsoft.AspNetCore.Components;
using System.Data;

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

        public string UpdateStatus { get; private set; }

        public bool ForecastsReady { get; private set; }

        public bool AppLoading;


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override async Task OnInitializedAsync()
        {
            AppLoading = true;
            this.eLogToConsole(nameof(OnInitializedAsync));

            await Task.Delay(1);

            UpdateStatus = "Loading weather data...";

            await Task.Delay(100000);


            //var weatherCompletion = new TaskCompletionSource<int>();
            //Dispatcher.Dispatch(new EnsureWeatherLoadedAction(false, weatherCompletion));

            //int numWeatherDaysUpdated = await weatherCompletion.Task;



            //UpdateStatus = "Loading gas readings...";
            //StateHasChanged();

            //await Task.Delay(1000);



            //var gasReadingsCompletion = new TaskCompletionSource<int>();
            //Dispatcher.Dispatch(new EnsureGasReadingsLoadedAction(false, gasReadingsCompletion));

            //int numGasReadingsUpdated = await gasReadingsCompletion.Task;
            //// dispatch analysis action

            //UpdateStatus = "Loading forecasts...";
            //StateHasChanged();

            //await Task.Delay(1000);

            //var forecastCompletion = new TaskCompletionSource<bool>();
            //Dispatcher.Dispatch
            //(
            //    new UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction
            //    (
            //        numGasReadingsUpdated,
            //        MeterType.Gas,
            //        forecastCompletion
            //    )
            //);

            //await forecastCompletion.Task;


            //UpdateStatus = "Ready";
            //StateHasChanged();

            //await Task.Delay(1000);

            AppLoading = false;

        }



    }
}
