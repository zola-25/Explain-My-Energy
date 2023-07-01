namespace Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;

public interface IMeterDataApi
{
    Task<MeterData> GetMeterData(Guid globalId);
    Task<AnalysisData> GetMeterAnalysisData(Guid globalId);
}