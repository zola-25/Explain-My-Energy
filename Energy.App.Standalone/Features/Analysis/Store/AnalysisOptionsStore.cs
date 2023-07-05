using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Setup.Store;
using Energy.Shared;
using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Features.Analysis.Store
{
    [PersistState]
    public record AnalysisOptionsState
    {
        public MeterAnalysisOptions this[MeterType meterType]
        {
            get
            {
                switch (meterType)
                {
                    case MeterType.Gas:
                        return Gas;
                    case MeterType.Electricity:
                        return Electricity;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(meterType), meterType, null);
                }
            }
        }

        public MeterAnalysisOptions Electricity { get; init; }

        public MeterAnalysisOptions Gas { get; init; }
    }

    public record MeterAnalysisOptions
    {
        public DateTime? HighlightStart { get; init; }
        public DateTime? HighlightEnd { get; init; }

        public bool HighlightSet { get; init; }

        public bool ShowCost { get; init; }
        public bool ShowKWh => !ShowCost;

        public decimal DegreeDifference { get; init; }

        public bool ChartRendered { get; init; }

        public CalendarTerm CalendarTerm { get; init; }

        public ToggleSource ToggleSource { get; init; }
    }

    public enum ToggleSource
    {
        None,
        Historical,
        Current,
        Forecast
    }


    public class AnalysisOptionsFeature : Feature<AnalysisOptionsState>
    {
        public override string GetName()
        {
            return nameof(AnalysisOptionsFeature);
        }

        protected override AnalysisOptionsState GetInitialState()
        {
            return new AnalysisOptionsState
            {
                Gas = new MeterAnalysisOptions
                {
                    CalendarTerm = CalendarTerm.Week,
                    DegreeDifference = 0,
                    ChartRendered = false,
                    HighlightSet = false,
                    HighlightStart = null,
                    HighlightEnd = null,
                    ShowCost = false,
                    ToggleSource = ToggleSource.None
                },
                Electricity = new MeterAnalysisOptions
                {
                    CalendarTerm = CalendarTerm.Week,
                    DegreeDifference = 0,
                    ChartRendered = false,
                    HighlightSet = false,
                    HighlightStart = null,
                    HighlightEnd = null,
                    ShowCost = false,
                    ToggleSource = ToggleSource.None
                },
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

    public class GasAnalysisOptionsSetChartRenderedAction : IAnalysisOptionsAction
    {
        public bool ChartRendered { get; }

        public GasAnalysisOptionsSetChartRenderedAction(bool chartRendered)
        {
            ChartRendered = chartRendered;
        }
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

        [ReducerMethod]
        public static AnalysisOptionsState OnGasRemoveHighlightRangeReducer(AnalysisOptionsState analysisOptionsState, GasAnalysisOptionsRemoveHighlightRangeAction removeHighlightRangeAction)
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
        public static AnalysisOptionsState OnGasSetChartRenderedReducer(AnalysisOptionsState analysisOptionsState, GasAnalysisOptionsSetChartRenderedAction action)
        {
            var meterState = analysisOptionsState.Gas;
            return analysisOptionsState with
            {
                Gas = meterState with
                {
                    ChartRendered = action.ChartRendered
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

        [ReducerMethod]
        public static AnalysisOptionsState OnElectricityRemoveHighlightRangeReducer(AnalysisOptionsState analysisOptionsState, ElectricityAnalysisOptionsRemoveHighlightRangeAction removeHighlightRangeAction)
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
        public static AnalysisOptionsState OnElectricitySetChartRenderedReducer(AnalysisOptionsState analysisOptionsState, ElectricityAnalysisOptionsSetChartRenderedAction action)
        {
            var meterState = analysisOptionsState.Electricity;
            return analysisOptionsState with
            {
                Electricity = meterState with
                {
                    ChartRendered = action.ChartRendered
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


    public interface IAnalysisOptionsAction { }

    public enum AnalysisAction
    {
        ShowCost,
        SetHighlightRange,
        RemoveHighlightRange,
        SetChartRendered,
        SetCalenderTerm,
        SetDegreeDifference
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
            Type type = Type.GetType(fullClassName);
            if (type == null)
            {
                throw new ArgumentException($"Type '{fullClassName}' not found.");
            }

            // Create an instance of the class
            return (IAnalysisOptionsAction)Activator.CreateInstance(type, constructorArgs);
        }
        public string GetThisNamespace()
        {
            return GetType().Namespace;
        }
    }

}
