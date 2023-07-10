using Energy.App.Standalone.Features.AppInit.Store;
using Fluxor;
using Fluxor.Blazor.Web.Components;

namespace Energy.App.Standalone.PageComponents
{
    public partial class DataValidatorBase : FluxorComponent
    {
        private AppStateValidator _appStateValidator;
        private IState<AppValidationState> _appValidationState;
        public TaskCompletionSource<bool> DataLoading { get; private set;} 

        public DataValidatorBase(AppStateValidator appStateValidator, IState<AppValidationState> appValidationState)
        {
            _appStateValidator = appStateValidator;
            _appValidationState = appValidationState;
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override async Task OnInitializedAsync()
        {
            DataLoading = new TaskCompletionSource<bool>();

            await base.OnInitializedAsync();

            _appStateValidator.Validate(DataLoading);


        }
    }
}
