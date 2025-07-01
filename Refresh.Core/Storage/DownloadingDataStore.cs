using System.Collections.Concurrent;
using Bunkum.Core.Storage;

namespace Refresh.Core.Storage;

public class DownloadingDataStore : IDataStore
{
    private readonly IDataStore _localDataStore;
    private readonly IDataStore _remoteDataStore;
    private readonly ConcurrentDictionary<string, int> _hits = [];

    private const int HitsBeforeDownload = 3;
    
    private const string WriteError = $"{nameof(DownloadingDataStore)} is a read-only source, and cannot be written to.";

    public DownloadingDataStore(IDataStore localDataStore, IDataStore remoteDataStore)
    {
        this._localDataStore = localDataStore;
        this._remoteDataStore = remoteDataStore;
    }

    private bool Hit(string key)
    {
        if (!this._hits.TryGetValue(key, out int value) && this._remoteDataStore.ExistsInStore(key))
        {
            _hits[key] = 1;
            return false;
        }

        value++;
        _hits[key] = value;

        return value >= HitsBeforeDownload;
    }

    
    public bool ExistsInStore(string key)
    {
        if (this._hits.ContainsKey(key))
            return true;

        return this._remoteDataStore.ExistsInStore(key);
    }

    public bool WriteToStore(string key, byte[] data)
    {
        throw new InvalidOperationException(WriteError);
    }

    public byte[] GetDataFromStore(string key)
    {
        byte[] data = this._remoteDataStore.GetDataFromStore(key);
        if (this.Hit(key))
        {
            this._hits.Remove(key, out _);
            this._localDataStore.WriteToStore(key, data);
        }

        return data;
    }

    public bool RemoveFromStore(string key)
    {
        throw new InvalidOperationException(WriteError);
    }

    public string[] GetKeysFromStore()
    {
        return this._remoteDataStore.GetKeysFromStore();
    }

    public bool WriteToStoreFromStream(string key, Stream data)
    {
        throw new InvalidOperationException(WriteError);
    }

    public Stream GetStreamFromStore(string key)
    {
        Stream data = this._remoteDataStore.GetStreamFromStore(key);
        if (this.Hit(key))
        {
            this._hits.Remove(key, out _);
            this._localDataStore.WriteToStoreFromStream(key, data);
            data.Seek(0, SeekOrigin.Begin);
        }

        return data;
    }

    public Stream OpenWriteStream(string key)
    {
        throw new InvalidOperationException(WriteError);
    }
}