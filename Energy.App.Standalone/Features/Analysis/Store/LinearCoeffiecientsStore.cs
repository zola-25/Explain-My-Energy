using Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;
using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.EnergyReadings.Store;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.App.Standalone.Features.Weather.Store;
using Energy.Shared;
using Fluxor;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Store
{
    public record LinearCoefficientsState
    { 
            
        public bool Saved { get; init; }
        public decimal Gradient { get; init; }
        public decimal C { get; init; }

        public decimal PredictTemperatureX(decimal yConsumption)
        {
            return (yConsumption - C) / Gradient;
        }

        public decimal PredictConsumptionY(decimal xTemperature)
        {
            return (Gradient * xTemperature) + C;
        }


        public IEnumerable<(decimal temperatureX, decimal consumptionY)> Plot(decimal xMinTemp, decimal xMaxTemp)
        {
            var interval = (xMaxTemp - xMinTemp) / 100m;

            return Enumerable.Range(0, 100)
                .Select(c =>
                {
                    var x = xMinTemp + interval * c;
                    return (x, PredictConsumptionY(x));
                });
        }
    }

    public class LinearCoeffiecientsFeature : Feature<LinearCoefficientsState>
    {
        public override string GetName()
        {
            return nameof(LinearCoeffiecientsFeature);
        }

        protected override LinearCoefficientsState GetInitialState()
        {

            return new LinearCoefficientsState()
            {   
                Saved = false,
                C = 0,
                Gradient = 0,
            };
        }
    }

    public class InitiateUpdateLinearCoefficientsAction
    { }

    public class NotifyLinearCoefficientsReadyAction
    { }

    public class UpdateLinearCoefficientsAction
    {
        public decimal Gradient { get; }
        public decimal C { get; }

        public UpdateLinearCoefficientsAction(decimal gradient, decimal c)
        {
            Gradient = gradient;
            C = c;
        }
    }
    
    public static class LinearCoeffiecientsReducer
    {
        [ReducerMethod]
        public static LinearCoefficientsState Reduce(LinearCoefficientsState state, UpdateLinearCoefficientsAction action)
        {
            return state with
            {
                Saved = true,
                Gradient = action.Gradient,
                C = action.C,
            };
        }
    }

    // generate an effects class
    // 

    public class LinearCoeffiecientsEffects
    {
        IState<GasReadingsState> _gasReadingsState;
        IState<ElectricityReadingsState> _electricityReadingsState;
        IState<HouseholdState> _householdState;
        IState<WeatherState> _weatherState;

        IForecastCoefficientsCreator _forecastCoefficientsCreator;
        public LinearCoeffiecientsEffects(IState<GasReadingsState> gasReadingsState, IState<ElectricityReadingsState> electricityReadingsState, IState<HouseholdState> householdState, IForecastCoefficientsCreator forecastCoefficientsCreator, IState<WeatherState> weatherState)
        {
            _gasReadingsState = gasReadingsState;
            _electricityReadingsState = electricityReadingsState;
            _householdState = householdState;
            _forecastCoefficientsCreator = forecastCoefficientsCreator;
            _weatherState = weatherState;
        }

        [EffectMethod]
        
        public async Task HandleInitiateUpdateLinearCoeffiecientsAction(InitiateUpdateLinearCoefficientsAction action, IDispatcher dispatcher)
        {
            ImmutableList<BasicReading> heatingMeterReadings;
            switch (_householdState.Value.PrimaryHeatSource)
            {
                case MeterType.Gas:
                    heatingMeterReadings = _gasReadingsState.Value.BasicReadings;
                    break;
                default:
                    heatingMeterReadings = _electricityReadingsState.Value.BasicReadings;
                    break;
            }

            var linearCoefficients = _forecastCoefficientsCreator
                .GetForecastCoefficients(heatingMeterReadings, _weatherState.Value.WeatherReadings);
            
            dispatcher.Dispatch(new UpdateLinearCoefficientsAction(linearCoefficients.Gradient, linearCoefficients.C));
            dispatcher.Dispatch(new NotifyLinearCoefficientsReadyAction());
        }

        [EffectMethod]
        public async Task HandleNotifyElectricityStoreReadyAction(NotifyElectricityStoreReady action, IDispatcher dispatcher)
        {
            if(_householdState.Value.PrimaryHeatSource == MeterType.Electricity)
                dispatcher.Dispatch(new InitiateUpdateLinearCoefficientsAction());
        }

        [EffectMethod]
        public async Task HandleNotifyGasStoreReadyAction(NotifyGasStoreReady action, IDispatcher dispatcher)
        {
            if (_householdState.Value.PrimaryHeatSource == MeterType.Gas)
                dispatcher.Dispatch(new InitiateUpdateLinearCoefficientsAction());
        }

    }
    
    
    

    










    
}
