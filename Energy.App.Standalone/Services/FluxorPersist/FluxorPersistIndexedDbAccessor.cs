using Microsoft.JSInterop;
using System;
using Energy.App.Standalone.Services.FluxorPersist;

namespace Energy.App.Standalone.Services.FluxorPersist;
public class FluxorPersistIndexedDbAccessor : IAsyncDisposable, IIndexedDbAccessor
{
    private Lazy<IJSObjectReference> _accessorJsRef = new();
    private readonly IJSRuntime _jsRuntime;

    public FluxorPersistIndexedDbAccessor(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        await WaitForReference();
        await _accessorJsRef.Value.InvokeVoidAsync("initialize");
    }

    private async Task WaitForReference()
    {
        if (_accessorJsRef.IsValueCreated is false)
        {
            _accessorJsRef = new(await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/FluxorPersistIndexedDb.js"));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }

    public async Task<IDbState> GetValueAsync(string stateName)
    {
        
        var result = await _jsRuntime.InvokeAsync<IDbState>("FluxorPersistIndexedDb.getState", stateName);

        return result;
    }

    public async Task SetValueAsync(IDbState dbState)
    {
        await _jsRuntime.InvokeVoidAsync("FluxorPersistIndexedDb.saveState", dbState.StateName);
    }
}