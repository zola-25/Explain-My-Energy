namespace Energy.App.Standalone.Features.Analysis.Services.DataLoading.Models
{
    public record CstR
    {
        public DateTime TApFrom { get; init; }
        public decimal TDStndP { get; init; }
        public decimal THHStndCh { get; init; }
        public decimal TPpKWh { get; init; }

        public DateTime UtcTime { get; init; }
        public decimal KWh { get; init; }
        public decimal CostP { get; init; } 
        public bool Fcst { get; init; }
        public bool Fixed { get; init; }
    }
}
