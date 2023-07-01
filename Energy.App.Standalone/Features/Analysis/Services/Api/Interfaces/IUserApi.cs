namespace Energy.App.Standalone.Features.Analysis.Services.Api.Interfaces
{
    public interface IUserApi
    {
        Task<UserDetails> EnsureCreated();
    }
}