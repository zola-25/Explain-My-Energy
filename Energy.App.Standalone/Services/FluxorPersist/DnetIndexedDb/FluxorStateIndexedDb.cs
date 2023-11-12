using DnetIndexedDb;
using Microsoft.JSInterop;

namespace Energy.App.Standalone.Services.FluxorPersist.DnetIndexedDb;

public class FluxorStateIndexedDb : IndexedDbInterop
{
    public FluxorStateIndexedDb(IJSRuntime jsRuntime, IndexedDbOptions indexedDbDatabaseOptions) : base(jsRuntime, indexedDbDatabaseOptions)
    {
    }


}
