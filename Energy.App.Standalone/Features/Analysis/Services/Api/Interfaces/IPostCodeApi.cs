namespace Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;

public interface IPostCodeApi
{
    Task<List<OutCode>> SearchOutCodes(string firstLetters, CancellationToken ctx = default);
}