using Energy.App.Standalone.Extensions;
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

        public bool WeatherLoaded { get; private set; }
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

            TaskCompletionSource<bool> weatherCompletion = new TaskCompletionSource<bool>();
            Dispatcher.Dispatch(new LoadWeatherAction(false, weatherCompletion));

            await weatherCompletion.Task;




            WeatherLoaded = true;
            AppLoading = false;
        }



    }
}
