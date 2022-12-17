namespace Refresh.HttpServer.Storage;

public class NullDataStore : IDataStore
{
    public bool ExistsInStore(string key) => false;
    public bool WriteToStore(string key, byte[] data) => false;
    public byte[] GetDataFromStore(string key) => throw new InvalidOperationException();
}