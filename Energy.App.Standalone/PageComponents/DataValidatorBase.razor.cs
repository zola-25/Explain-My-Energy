using Energy.App.Standalone.Extensions;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Analysis.Store.HistoricalForecast.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Electricity.Actions;
using Energy.App.Standalone.Features.EnergyReadings.Gas.Actions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.App.Standalone.Features.Setup.Weather.Store;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using MudBlazor;

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

            UpdateStatus = "Loading data...";
            await Task.Delay(1);
            StateHasChanged();

            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;

            SubscribeToAction<NotifyWeatherLoadingFinished>((action) =>
            {
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);
            });

            SubscribeToAction<NotifyGasLoadingFinished>((action) =>
            {
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);

            });

            SubscribeToAction<NotifyElectricityLoadingFinished>((action) =>
            {
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);
            });

            SubscribeToAction<NotifyHeatingForecastUpdatedAction>((action) =>
            {
                Snackbar.Add("Temperature-Consumption Forecast Ready", Severity.Info);
            });
            SubscribeToAction<NotifyElectricityForecastResult>((action) =>
            {
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);
            });
            SubscribeToAction<NotifyGasForecastResult>((action) =>
            {
                Snackbar.Add(action.Message, action.Success ? Severity.Info : Severity.Warning);
            });

            var weatherReadingCompletion = new TaskCompletionSource<(bool, string)>();
            Dispatcher.Dispatch(new EnsureWeatherLoadedAction(false, weatherReadingCompletion));

            UpdateStatus = "Loading energy readings...";
            await Task.Delay(1);
            StateHasChanged();

            var gasReadingsCompletion = new TaskCompletionSource<(bool, string)>();

            Dispatcher.Dispatch(new EnsureGasReadingsLoadedAction(false, gasReadingsCompletion));

            var electricityReadingsCompletion = new TaskCompletionSource<(bool, string)>();
            Dispatcher.Dispatch(new EnsureElectricityReadingsLoadedAction(false, electricityReadingsCompletion));



            var electricityForecastCompletion = new TaskCompletionSource<(bool, string)>();
            Dispatcher.Dispatch(new EnsureElectricityHistoricalForecastAction(false, electricityForecastCompletion));

            var gasForecastCompletion = new TaskCompletionSource<(bool, string)>();
            Dispatcher.Dispatch(new EnsureGasHistoricalForecastAction(false, gasForecastCompletion));


            var heatingForecastCompletion = new TaskCompletionSource<(bool, string)>();
            Dispatcher.Dispatch
            (
                new EnsureCoeffsAndHeatingForecastLoaded
                (
                    0,
                    heatingForecastCompletion
                )
            );

            await Task.WhenAll(weatherReadingCompletion.Task,
             gasReadingsCompletion.Task,
             electricityReadingsCompletion.Task,
             electricityForecastCompletion.Task,
             gasForecastCompletion.Task,
             heatingForecastCompletion.Task);

            await Task.Delay(1);
            StateHasChanged();
            UpdateStatus = "Ready";

            AppLoading = false;
        }
    }
}
