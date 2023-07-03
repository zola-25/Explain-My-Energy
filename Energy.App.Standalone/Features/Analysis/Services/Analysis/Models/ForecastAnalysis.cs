﻿using Energy.App.Standalone.Extensions;

namespace Energy.App.Standalone.Features.Analysis.Services.Analysis.Models;

public record ForecastAnalysis
{
    public string DateText => $"{Start.eDateToMinimal()} - {End.eDateToMinimal()}";
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
    public decimal ForecastCostPence { get; init; }
    public decimal ForecastConsumption { get; init; }
    public int NumberOfDays { get; init; }
    public decimal ForecastCo2 { get; init; }
    public TemperatureRange TemperatureRange { get; init; }
}
