using Energy.App.Standalone.Services.FluxorPersist;

public interface IIndexedDbAccessor
{
    ValueTask DisposeAsync();
    Task<IDbState> GetValueAsync(string stateName);
    Task InitializeAsync();
    Task SetValueAsync(IDbState dbState);
}