using Bunkum.Core.Storage;

namespace RefreshTests.GameServer.GameServer;

/// <summary>
/// A data store that you are unable to write to, it always returns a failure operation
/// </summary>
public class WriteFailingDataStore : IDataStore
{
    public bool ExistsInStore(string key) => false;
    public bool WriteToStore(string key, byte[] data) => false;
    public byte[] GetDataFromStore(string key) => throw new NotSupportedException();
    public bool RemoveFromStore(string key) => throw new NotSupportedException();
    public string[] GetKeysFromStore() => Array.Empty<string>();
    public bool WriteToStoreFromStream(string key, Stream data) => false;
    public Stream GetStreamFromStore(string key) => throw new NotSupportedException();
    public Stream OpenWriteStream(string key) => throw new NotSupportedException();
}