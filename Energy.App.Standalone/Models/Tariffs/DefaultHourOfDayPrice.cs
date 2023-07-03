namespace Energy.App.Standalone.Models.Tariffs;

public record DefaultHourOfDayPrice
{

    public TimeSpan HourOfDay { get; init; }
    public decimal PencePerKWh { get; init; }

}