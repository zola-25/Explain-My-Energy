using Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;
using Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models;
using Energy.Shared;
using System.Collections.Immutable;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Interfaces;

public interface IHistoricalDurationAnalyzer
{
    HistoricalAnalysis GetCurrentDurationAnalysis(MeterType meterType, CalendarTerm duration, ImmutableList<CostedReading> readings, ImmutableList<DailyWeatherReading> dailyWeatherReadings);
    HistoricalAnalysis GetPreviousDurationAnalysis(MeterType meterType, CalendarTerm duration, ImmutableList<CostedReading> readings, ImmutableList<DailyWeatherReading> dailyWeatherReadings);
}