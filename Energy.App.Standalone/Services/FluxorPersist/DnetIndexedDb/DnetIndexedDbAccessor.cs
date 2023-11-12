namespace Energy.App.Standalone.Services.FluxorPersist.DnetIndexedDb;

public class DnetIndexedDbAccessor : IIndexedDbAccessor
{
    private readonly FluxorStateIndexedDb FluxorStateIndexedDb;
    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IDbState> GetValueAsync(string stateName)
    {
        var result = await FluxorStateIndexedDb.AddBlobItem<string>;
        
    }

    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public Task SetValueAsync(IDbState dbState)
    {
        throw new NotImplementedException();
    }
}
