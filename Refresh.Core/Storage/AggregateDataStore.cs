using System.Collections.Frozen;
using Bunkum.Core.Storage;

namespace Refresh.Core.Storage;

public class AggregateDataStore : IDataStore
{
    private IDataStore PrimaryStore { get; }
    private FrozenSet<IDataStore> AllStores { get; }
    
    public AggregateDataStore(IDataStore primary, params IDataStore[] others)
    {
        this.PrimaryStore = primary;
        
        List<IDataStore> stores = new(others.Length + 1) { primary };
        stores.AddRange(others);
        
        this.AllStores = stores.ToFrozenSet();
    }
    
    private IDataStore GetStoreContainingKey(string key)
    {
        IDataStore? store = this.AllStores.FirstOrDefault(s => s.ExistsInStore(key));
        
        if (store == null)
            throw new InvalidOperationException($"No data stores contained the key '{key}'.");
        
        return store;
    }
    
    public bool ExistsInStore(string key) => this.AllStores.Any(s => s.ExistsInStore(key));
    public byte[] GetDataFromStore(string key) => this.GetStoreContainingKey(key).GetDataFromStore(key);
    public Stream GetStreamFromStore(string key) => this.GetStoreContainingKey(key).GetStreamFromStore(key);
    
    public string[] GetKeysFromStore() => this.AllStores.SelectMany(r => r.GetKeysFromStore()).ToArray();
    
    public bool WriteToStore(string key, byte[] data) => this.PrimaryStore.WriteToStore(key, data);
    public bool RemoveFromStore(string key) => this.PrimaryStore.RemoveFromStore(key);
    
    public Stream OpenWriteStream(string key) => this.PrimaryStore.OpenWriteStream(key);
    public bool WriteToStoreFromStream(string key, Stream data) => this.PrimaryStore.WriteToStoreFromStream(key, data);
}