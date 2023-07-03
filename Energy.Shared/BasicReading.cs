namespace Energy.Shared
{
    public record BasicReading
    {
        public decimal KWh { get; init; }
        public DateTime UtcTime { get; init; }
        public bool Forecast { get; init; }
    }
}
