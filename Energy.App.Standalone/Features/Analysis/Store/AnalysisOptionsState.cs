using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Store.HeatingForecast.Actions;
using Energy.App.Standalone.Features.Setup.Household;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;
using System.Text.Json.Serialization;

namespace Energy.App.Standalone.Features.Analysis.Store
{
    [PersistState]
    [FeatureState(Name = nameof(AnalysisOptionsState))]
    public record AnalysisOptionsState
    {
        [property: JsonIgnore]
        public MeterAnalysisOptions this[MeterType meterType]
        {
            get
            {
                return meterType switch
                {
                    MeterType.Gas => Gas,
                    MeterType.Electricity => Electricity,
                    _ => throw new ArgumentOutOfRangeException(nameof(meterType), meterType, null),
                };
            }
        }

        [property: JsonIgnore]
        public IEnumerable<MeterAnalysisOptions> All => new[] { Electricity, Gas };

        public MeterAnalysisOptions Electricity { get; init; }

        public MeterAnalysisOptions Gas { get; init; }

        public AnalysisOptionsState()
        {

            Gas = new MeterAnalysisOptions
            {
                MeterType = MeterType.Gas,
                CalendarTerm = CalendarTerm.Month,
                DegreeDifference = 0,
                HighlightSet = false,
                HighlightStart = null,
                HighlightEnd = null,
                ShowCost = false,
                ToggleSource = ToggleSource.None,
                UseHistoricalForecast = false
            };
            Electricity = new MeterAnalysisOptions
            {
                MeterType = MeterType.Electricity,
                CalendarTerm = CalendarTerm.Month,
                DegreeDifference = 0,
                HighlightSet = false,
                HighlightStart = null,
                HighlightEnd = null,
                ShowCost = false,
                ToggleSource = ToggleSource.None,
                UseHistoricalForecast = true
            };
        }
    }

    public record MeterAnalysisOptions
    {
        public MeterType MeterType { get; init; }
        public DateTime? HighlightStart { get; init; }
        public DateTime? HighlightEnd { get; init; }

        public bool HighlightSet { get; init; }

        public bool ShowCost { get; init; }
        public bool ShowKWh => !ShowCost;

        public decimal DegreeDifference { get; init; }

        public CalendarTerm CalendarTerm { get; init; }

        public ToggleSource ToggleSource { get; init; }
        public bool UseHistoricalForecast { get; init; }
    }

    public enum ToggleSource
    {
        None,
        Historical,
        Current,
        Forecast
    }

    public class GasAnalysisOptionsUseHistoricalForecastAction : IAnalysisOptionsAction
    {
        public bool UseHistoricalForecast { get; }

        public GasAnalysisOptionsUseHistoricalForecastAction(bool useHistoricalForecast)
        {
            UseHistoricalForecast = useHistoricalForecast;
        }

        [ReducerMethod]
        public static AnalysisOptionsState Reduce(AnalysisOptionsState state, GasAnalysisOptionsUseHistoricalForecastAction action)
        {
            return state with
            {
                Gas = state.Gas with
                {
                    UseHistoricalForecast = action.UseHistoricalForecast
                }
            };
        }
    }

    public class ElectricityAnalysisOptionsUseHistoricalForecastAction : IAnalysisOptionsAction
    {
        public bool UseHistoricalForecast { get; }

        public ElectricityAnalysisOptionsUseHistoricalForecastAction(bool useHistoricalForecast)
        {
            UseHistoricalForecast = useHistoricalForecast;
        }

        [ReducerMethod]
        public static AnalysisOptionsState Reduce(AnalysisOptionsState state, ElectricityAnalysisOptionsUseHistoricalForecastAction action)
        {
            return state with
            {
                Electricity = state.Electricity with
                {
                    UseHistoricalForecast = action.UseHistoricalForecast
                }
            };
        }
    }

    public class GasAnalysisOptionsShowCostAction : IAnalysisOptionsAction
    {
        public bool ShowCost { get; }

        public GasAnalysisOptionsShowCostAction(bool showCost)
        {
            ShowCost = showCost;
        }
    }

    public class GasAnalysisOptionsSetDegreeDifferenceAction : IAnalysisOptionsAction
    {
        public decimal DegreeDifference { get; }

        public GasAnalysisOptionsSetDegreeDifferenceAction(decimal degreeDifference)
        {
            DegreeDifference = degreeDifference;
        }
    }



    public class ElectricityAnalysisOptionsSetDegreeDifferenceAction : IAnalysisOptionsAction
    {
        public decimal DegreeDifference { get; }

        public ElectricityAnalysisOptionsSetDegreeDifferenceAction(decimal degreeDifference)
        {
            DegreeDifference = degreeDifference;
        }
    }

    public class GasAnalysisOptionsSetHighlightRangeAction : IAnalysisOptionsAction
    {

        public GasAnalysisOptionsSetHighlightRangeAction(ToggleSource toggleSource, DateTime start, DateTime end)
        {
            ToggleSource = toggleSource;
            Start = start;
            End = end;
        }

        public ToggleSource ToggleSource { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
    }

    public class GasAnalysisOptionsRemoveHighlightRangeAction : IAnalysisOptionsAction
    {
    }

    

    public class GasAnalysisOptionsSetCalenderTermAction : IAnalysisOptionsAction
    {

        public GasAnalysisOptionsSetCalenderTermAction(CalendarTerm calendarTerm)
        {
            CalendarTerm = calendarTerm;
        }

        public CalendarTerm CalendarTerm { get; }
    }


    public class ElectricityAnalysisOptionsShowCostAction : IAnalysisOptionsAction
    {
        public bool ShowCost { get; }

        public ElectricityAnalysisOptionsShowCostAction(bool showCost)
        {
            ShowCost = showCost;
        }
    }

    public class ElectricityAnalysisOptionsSetHighlightRangeAction : IAnalysisOptionsAction
    {

        public ElectricityAnalysisOptionsSetHighlightRangeAction(ToggleSource toggleSource, DateTime start, DateTime end)
        {
            ToggleSource = toggleSource;
            Start = start;
            End = end;
        }

        public ToggleSource ToggleSource { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
    }

    public class ElectricityAnalysisOptionsRemoveHighlightRangeAction : IAnalysisOptionsAction
    {
    }

    public class ElectricityAnalysisOptionsSetChartRenderedAction : IAnalysisOptionsAction
    {
        public bool ChartRendered { get; }

        public ElectricityAnalysisOptionsSetChartRenderedAction(bool chartRendered)
        {
            ChartRendered = chartRendered;
        }
    }

    public class ElectricityAnalysisOptionsSetCalenderTermAction : IAnalysisOptionsAction
    {

        public ElectricityAnalysisOptionsSetCalenderTermAction(CalendarTerm calendarTerm)
        {
            CalendarTerm = calendarTerm;
        }

        public CalendarTerm CalendarTerm { get; }
    }



    public static class AnalysisOptionsReducers
    {
        [ReducerMethod]
        public static AnalysisOptionsState OnGasShowCostReducer(AnalysisOptionsState analysisOptionsState, GasAnalysisOptionsShowCostAction showCostAction)
        {
            var meterState = analysisOptionsState.Gas;
            return analysisOptionsState with
            {
                Gas = meterState with
                {
                    MeterType = MeterType.Gas,
                    ShowCost = showCostAction.ShowCost
                }
            };

        }

        [ReducerMethod]
        public static AnalysisOptionsState OnGasShowHighlightRangeReducer(AnalysisOptionsState analysisOptionsState, GasAnalysisOptionsSetHighlightRangeAction setHighlightRange)
        {
            var meterState = analysisOptionsState.Gas;
            return analysisOptionsState with
            {
                Gas = meterState with
                {
                    HighlightStart = setHighlightRange.Start,
                    HighlightEnd = setHighlightRange.End,
                    HighlightSet = true,
                    ToggleSource = setHighlightRange.ToggleSource
                }
            };

        }

        [ReducerMethod(typeof(GasAnalysisOptionsRemoveHighlightRangeAction))]
        public static AnalysisOptionsState OnGasRemoveHighlightRangeReducer(AnalysisOptionsState analysisOptionsState)
        {
            var meterState = analysisOptionsState.Gas;
            return analysisOptionsState with
            {
                Gas = meterState with
                {
                    HighlightStart = null,
                    HighlightEnd = null,
                    HighlightSet = false,
                    ToggleSource = ToggleSource.None
                }
            };

        }


        [ReducerMethod]
        public static AnalysisOptionsState OnGasSetCalenderTermReducer(AnalysisOptionsState analysisOptionsState, GasAnalysisOptionsSetCalenderTermAction action)
        {
            var meterState = analysisOptionsState.Gas;
            return analysisOptionsState with
            {
                Gas = meterState with
                {
                    CalendarTerm = action.CalendarTerm
                }
            };
        }

        [ReducerMethod]
        public static AnalysisOptionsState OnElectricityShowCostReducer(AnalysisOptionsState analysisOptionsState, ElectricityAnalysisOptionsShowCostAction showCostAction)
        {
            var meterState = analysisOptionsState.Electricity;
            return analysisOptionsState with
            {
                Electricity = meterState with
                {
                    MeterType = MeterType.Electricity,
                    ShowCost = showCostAction.ShowCost
                }
            };

        }

        [ReducerMethod]
        public static AnalysisOptionsState OnElectricityShowHighlightRangeReducer(AnalysisOptionsState analysisOptionsState, ElectricityAnalysisOptionsSetHighlightRangeAction setHighlightRange)
        {
            var meterState = analysisOptionsState.Electricity;
            return analysisOptionsState with
            {
                Electricity = meterState with
                {
                    HighlightStart = setHighlightRange.Start,
                    HighlightEnd = setHighlightRange.End,
                    HighlightSet = true,
                    ToggleSource = setHighlightRange.ToggleSource
                }
            };

        }

        [ReducerMethod(typeof(ElectricityAnalysisOptionsRemoveHighlightRangeAction))]
        public static AnalysisOptionsState OnElectricityRemoveHighlightRangeReducer(AnalysisOptionsState analysisOptionsState)
        {
            var meterState = analysisOptionsState.Electricity;
            return analysisOptionsState with
            {
                Electricity = meterState with
                {
                    HighlightStart = null,
                    HighlightEnd = null,
                    HighlightSet = false,
                    ToggleSource = ToggleSource.None
                }
            };

        }


        [ReducerMethod]
        public static AnalysisOptionsState OnElectricitySetCalenderTermReducer(AnalysisOptionsState analysisOptionsState, ElectricityAnalysisOptionsSetCalenderTermAction action)
        {
            var meterState = analysisOptionsState.Electricity;
            return analysisOptionsState with
            {
                Electricity = meterState with
                {
                    CalendarTerm = action.CalendarTerm
                }
            };
        }

        [ReducerMethod]
        // Generate degree difference reducer
        public static AnalysisOptionsState OnElectricityDegreeDifferenceUpdateReducer(AnalysisOptionsState analysisOptionsState, ElectricityAnalysisOptionsSetDegreeDifferenceAction action )
        {
            var meterState = analysisOptionsState.Electricity;
            return analysisOptionsState with
            {
                Electricity = meterState with
                {
                    DegreeDifference = action.DegreeDifference
                }
            };
        }

        [ReducerMethod]
        // Generate gas degree difference reducer
        public static AnalysisOptionsState OnGasGenerateDegreeDifferenceReducer(AnalysisOptionsState analysisOptionsState, GasAnalysisOptionsSetDegreeDifferenceAction action)
        {
            var meterState = analysisOptionsState.Gas;
            return analysisOptionsState with
            {
                Gas = meterState with
                {
                    DegreeDifference = action.DegreeDifference
                }
            };
        }
    }

    public class AnalysisOptionsEffects
    {
        private readonly IState<HouseholdState> _householdState;

        public AnalysisOptionsEffects(IState<HouseholdState> householdState)
        {
            _householdState = householdState;
        }

        [EffectMethod]
        public Task OnElectricitySetDegreeDifferenceAction(ElectricityAnalysisOptionsSetDegreeDifferenceAction action, IDispatcher dispatcher)
        {
            if (_householdState.Value.PrimaryHeatSource == MeterType.Electricity) {
                dispatcher.Dispatch(new LoadHeatingForecastAction(action.DegreeDifference));
            }
            return Task.CompletedTask;
        }
        
        [EffectMethod]
        public Task OnGasSetDegreeDifferenceAction(GasAnalysisOptionsSetDegreeDifferenceAction action, IDispatcher dispatcher)
        {
            if (_householdState.Value.PrimaryHeatSource == MeterType.Gas) {
                dispatcher.Dispatch(new LoadHeatingForecastAction(action.DegreeDifference));
            }
            return Task.CompletedTask;
        }
    }


    public interface IAnalysisOptionsAction { }

    public enum AnalysisAction
    {
        ShowCost,
        SetHighlightRange,
        RemoveHighlightRange,
        SetChartRendered,
        SetCalenderTerm,
        SetDegreeDifference,
        UseHistoricalForecast
    }

    public class AnalysisOptionsActionFactory
    {
        private readonly MeterType _meterType;

        public AnalysisOptionsActionFactory(MeterType meterType)
        {
            _meterType = meterType;
        }

        public IAnalysisOptionsAction Create(AnalysisAction action, params object[] constructorArgs)
        {
            // Convert the enum value to the appropriate string (PascalCase) for constructing the class name
            string actionName = action.ToString();

            // Build the full class name based on MeterType and ActionName
            string className = $"{_meterType}AnalysisOptions{actionName}Action";

            // Assuming all classes are in the same assembly and namespace
            string fullClassName = $"{GetThisNamespace()}.{className}";

            // Get the Type based on the class name
            var type = Type.GetType(fullClassName) ?? throw new ArgumentException($"Type '{fullClassName}' not found.");

            // Create an instance of the class
            return (IAnalysisOptionsAction)Activator.CreateInstance(type, constructorArgs);
        }
        public string GetThisNamespace()
        {
            return GetType().Namespace;
        }
    }

}
