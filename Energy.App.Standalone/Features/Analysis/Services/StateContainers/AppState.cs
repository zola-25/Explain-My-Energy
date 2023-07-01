using System;
using Energy.App.Blazor.Client.Services.DataLoading.Interfaces;

namespace Energy.App.Blazor.Client.StateContainers
{
    public class AppState
    {
        private readonly UserState _userState;
        private readonly MeterState _meterState;
        private readonly WeatherDataState _weatherDataState;
        private readonly IEnergyDataProcessor _energyDataProcessor;

        public AppState(UserState userState,
                        MeterState meterState,
                        WeatherDataState weatherDataState,
                        IEnergyDataProcessor energyDataProcessor)
        {
            _userState = userState;
            _meterState = meterState;
            _weatherDataState = weatherDataState;
            _energyDataProcessor = energyDataProcessor;
        }

        public async Task Initialize()
        {
             UpdateInitializationState(InitializationState.RetrievingUserSetup);

            await _userState.Load();

            if (_userState.UserDetails.HouseholdSetupComplete)
            {
                 UpdateInitializationState(InitializationState.RetrievingMeterSetup);

                await _meterState.LoadAll();

                 UpdateInitializationState(InitializationState.RetrievingWeatherData);

                await _weatherDataState.LoadWeatherData();

                 UpdateInitializationState(InitializationState.UpdatingMeterData);

                var meters = _meterState.GetAll();
                foreach (var meter in meters)
                {
                    await _energyDataProcessor.UpdateData(meter);
                }
            }

             UpdateInitializationState(InitializationState.Completed);

            if (NotifyAppInitialized != null)
            {
                await NotifyAppInitialized.Invoke();
            }
        }

        private void UpdateInitializationState(InitializationState initializationState)
        {
            InitializationState = initializationState;
            NotifyAppInitializing?.Invoke(this, initializationState);
        }

        public bool Initializing => InitializationState != InitializationState.Completed;
        public InitializationState InitializationState { get; private set;}


        public bool IsInitialized => InitializationState == InitializationState.Completed;

        public event EventHandler<InitializationState> NotifyAppInitializing;


        public event Func<Task> NotifyAppInitialized;
    }

    public enum InitializationState
    {
        RetrievingUserSetup,
        RetrievingMeterSetup,
        RetrievingWeatherData,
        UpdatingMeterData,
        Completed
    }
}
