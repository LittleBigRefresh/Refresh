using Bunkum.Core.Storage;

namespace RefreshTests.GameServer.GameServer;

/// <summary>
/// A data store that claims keys are always available, but returns failure when getting the data back
/// </summary>
public class ReadFailingDataStore : IDataStore
{
    public bool ExistsInStore(string key) => true;
    public bool WriteToStore(string key, byte[] data) => true;
    public byte[] GetDataFromStore(string key) => throw new NotSupportedException(); 
    public bool RemoveFromStore(string key) => throw new NotSupportedException();
    public string[] GetKeysFromStore() => throw new NotSupportedException();
    public bool WriteToStoreFromStream(string key, Stream data) => true;
    public Stream GetStreamFromStore(string key) => throw new NotSupportedException();
    public Stream OpenWriteStream(string key) => throw new NotSupportedException();
}