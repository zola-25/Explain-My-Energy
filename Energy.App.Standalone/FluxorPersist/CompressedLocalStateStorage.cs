using Blazored.LocalStorage;
using Fluxor.Persist.Storage;
using LZStringCSharp;

namespace Energy.App.Standalone.FluxorPersist
{
    public class CompressedLocalStateStorage : IStringStateStorage
    {

        private ILocalStorageService LocalStorage { get; set; }

        public CompressedLocalStateStorage(ILocalStorageService localStorage)
        {
            LocalStorage = localStorage;
        }

        public async ValueTask<string> GetStateJsonAsync(string statename)
        {
            var compressed = await LocalStorage.GetItemAsStringAsync(statename);
            if (compressed == null)
            {
                return null;
            }
            return LZString.DecompressFromUTF16(compressed);
        }

        public async ValueTask StoreStateJsonAsync(string statename, string json)
        {
            if(json == null)
            {
                await LocalStorage.RemoveItemAsync(statename);
                return;
            }
            var compressed = LZString.CompressToUTF16(json);
            await LocalStorage.SetItemAsStringAsync(statename, compressed);
        }
    }
}
