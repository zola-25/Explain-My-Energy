using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Energy.Shared;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Fluxor.Persist.Middleware;
using Microsoft.AspNetCore.Components;
using MudBlazor.Interfaces;
using System.Data;

namespace Energy.App.Standalone.PageComponents
{
    public partial class DataValidatorBase : FluxorComponent
    {
        [Inject]
        private IDispatcher Dispatcher { get; set; }


        [Parameter]
        public RenderFragment ChildContent { get; set; }

        public string UpdateStatus { get; private set; }


        public bool AppLoading;

        [Inject]
        IState<HouseholdState> HouseholdState { get; set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override async Task OnInitializedAsync()
        {
            AppLoading = true;
            this.eLogToConsole(nameof(OnInitializedAsync));

            UpdateStatus = "Loading weather data...";
            await Task.Delay(1);
            StateHasChanged();

            var weatherCompletion = new TaskCompletionSource<int>();
            Dispatcher.Dispatch(new EnsureWeatherLoadedAction(false, weatherCompletion));

            int numWeatherDaysUpdated = await weatherCompletion.Task;
            

            UpdateStatus = "Loading gas readings...";
            await Task.Delay(1);
            StateHasChanged();


            var gasReadingsCompletion = new TaskCompletionSource<int>();
            Dispatcher.Dispatch(new EnsureGasReadingsLoadedAction(false, gasReadingsCompletion));
            int numGasReadingsUpdated = await gasReadingsCompletion.Task;


            UpdateStatus = "Loading electricity readings...";
            await Task.Delay(1);
            StateHasChanged();

            var electricityReadingsCompletion = new TaskCompletionSource<int>();
            Dispatcher.Dispatch(new EnsureElectricityReadingsLoadedAction(false, electricityReadingsCompletion));

            int numElectricityReadingsUpdated = await electricityReadingsCompletion.Task;


            UpdateStatus = "Loading historical forecasts...";
            await Task.Delay(1);
            StateHasChanged();

            var electricityHistoricalForecastCompletion = new TaskCompletionSource<(bool, string)>();
            var gasHistoricalForecastCompletion = new TaskCompletionSource<(bool, string)>();

            Dispatcher.Dispatch(new EnsureElectricityHistoricalForecastAction(false, electricityHistoricalForecastCompletion));

            Dispatcher.Dispatch(new EnsureGasHistoricalForecastAction(false, gasHistoricalForecastCompletion));


            await electricityHistoricalForecastCompletion.Task;
            await gasHistoricalForecastCompletion.Task;



            UpdateStatus = "Loading weather dependent forecasts...";
            await Task.Delay(1);
            StateHasChanged();


            int numHeatingReadingsUpdated = 
                HouseholdState.Value.PrimaryHeatSource
                    switch
                    {
                        MeterType.Gas => numGasReadingsUpdated,
                        MeterType.Electricity => numElectricityReadingsUpdated,
                        _ => throw new NotImplementedException()
                    };

            var forecastCompletion = new TaskCompletionSource<bool>();
            Dispatcher.Dispatch
            (
                new UpdateCoeffsAndOrForecastsIfSignificantOrOutdatedAction
                (
                    numHeatingReadingsUpdated,
                    forecastCompletion
                )
            );

            await forecastCompletion.Task;


            UpdateStatus = "Ready";
            await Task.Delay(1);
            StateHasChanged();

            AppLoading = false;
        }



    }
}
