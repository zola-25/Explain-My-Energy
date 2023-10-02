namespace Energy.App.Standalone.DTOs.Tariffs;

public record DefaultHourOfDayPrice
{

    public TimeSpan HourOfDay { get; init; }
    public decimal PencePerKWh { get; init; }
}