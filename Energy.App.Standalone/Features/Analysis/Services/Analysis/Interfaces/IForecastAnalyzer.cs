using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Store;
using Energy.App.Standalone.Features.Setup.Store.ImmutatableStateObjects;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;



public interface ITempForecastAnalyzer
{
    ForecastAnalysis GetCurrentPeriodForecastTotals(MeterType meterType, CalendarTerm duration, decimal degreeDifference, LinearCoefficientsState linearCoefficientsState, ImmutableList<TariffDetailState> tariffDetailStates, ImmutableList<DailyWeatherReading> dailyWeatherReadings);
    ForecastAnalysis GetNextPeriodForecastTotals(MeterType meterType, CalendarTerm duration, decimal degreeDifference, LinearCoefficientsState linearCoefficientsState, ImmutableList<TariffDetailState> tariffDetailStates, ImmutableList<DailyWeatherReading> dailyWeatherReadings);
}

public interface ISimpleForecastAnalyzer
{

}