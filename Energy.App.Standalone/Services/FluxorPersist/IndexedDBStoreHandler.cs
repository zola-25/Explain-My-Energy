using Fluxor;
using Fluxor.Persist.Storage;

namespace Energy.App.Standalone.Services.FluxorPersist;

public class IndexedDBStoreHandler : IStoreHandler
{
    private readonly IIndexedDbAccessor _indexedDbAccessor;
    private readonly ILogger<IndexedDBStoreHandler> _logger;

    public IndexedDBStoreHandler(IIndexedDbAccessor indexedDbAccessor, ILogger<IndexedDBStoreHandler> logger)
    {
        _indexedDbAccessor = indexedDbAccessor;
        _logger = logger;
    }

    public async Task<object> GetState(IFeature feature)
    {
        _logger?.LogDebug("Rehydrating state {FeatureName}", feature.GetName());
        object state = await _indexedDbAccessor.GetValueAsync(feature.GetName());
        if (state is null)
        {
            _logger?.LogDebug("No saved state for IndexedDb {FeatureName}, skipping", feature.GetName());
        }
        else
        {
            try
            {
                _logger?.LogDebug("Returning IndexedDb type {StateType}", feature.GetStateType());


                return state;

            }
            catch (Exception exception)
            {
                _logger?.LogError(exception, "Failed to return IndexedDb state. Skipping.");
            }
        }

        return feature.GetState();
    }

    public async Task SetState(IFeature feature)
    {
        try
        {
            object state = feature.GetState();

            var dbState = new DbState {
                State = state,
                StateName = feature.GetName()
            };

            await _indexedDbAccessor.SetValueAsync(dbState);
        }
        catch (Exception exception)
        {
            _logger?.LogError(exception, "Failed to set IndexedDb state. Skipping.");
        }
    }
}
