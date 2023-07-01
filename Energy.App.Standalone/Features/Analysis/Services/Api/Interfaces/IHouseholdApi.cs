namespace Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces;

public interface IHouseholdApi
{
    Task<HouseholdDetails> GetHouseholdForUser();
    Task AddOrUpdateAsync(HouseholdDetails householdDetails);
}