using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis
{
    public class AnalysisOptions
    {

        public AnalysisOptions()
        {
            Electricity = new MeterAnalysisOptions();
            Gas = new MeterAnalysisOptions();
        }

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

        public MeterAnalysisOptions Electricity { get; }

        public MeterAnalysisOptions Gas { get; }

    }

    public class MeterAnalysisOptions
    {
        public DateTime? HighlightStart { get; private set; }
        public DateTime? HighlightEnd { get; private set; }

        public bool HighlightSet { get; private set; }

        public bool ShowCost { get; private set; }
        public bool ShowKWh => !ShowCost;

        public async Task ToggleShowCost()
        {
            ShowCost = !ShowCost;

            if (NotifyToggleCostChange != null)
            {
                await NotifyToggleCostChange(ShowCost);
            }
        }

        public event Func<bool, Task> NotifyToggleCostChange;


        public async Task SetHighlightRange(DateTime start, DateTime end)
        {
            HighlightStart = start;
            HighlightEnd = end;
            HighlightSet = true;

            if (NotifySetHighlightRange != null)
            {
                await NotifySetHighlightRange(start, end);
            }
        }

        public async Task RemoveHighlightRange()
        {
            HighlightStart = null;
            HighlightEnd = null;
            HighlightSet = false;

            if (NotifyRemoveHighlightRange != null)
            {
                await NotifyRemoveHighlightRange();
            }
        }

        public event Func<Task> NotifyRemoveHighlightRange;

        public event Func<DateTime, DateTime, Task> NotifySetHighlightRange;



        public bool ChartRendered { get; private set; }
        public async Task SetChartRendered(bool isRendered)
        {
            ChartRendered = isRendered;
            if (NotifyChartRendered != null)
            {
                await NotifyChartRendered(isRendered);
            }
        }

        public event Func<bool, Task> NotifyChartRendered;

        public CalendarTerm CalendarTerm { get; private set; }


        public async Task SetTerm(CalendarTerm term)
        {
            if (term == CalendarTerm)
            {
                return;
            }

            CalendarTerm = term;

            if (NotifyTermChanged != null)
            {
                await NotifyTermChanged(term);
            }
        }

        public event Func<CalendarTerm, Task> NotifyTermChanged;

    }
}
