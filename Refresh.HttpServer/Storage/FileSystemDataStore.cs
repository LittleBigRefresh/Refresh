namespace Refresh.HttpServer.Storage;

public class FileSystemDataStore : IDataStore
{
    private static readonly string DataStoreDirectory = "dataStore" + Path.DirectorySeparatorChar;

    public FileSystemDataStore()
    {
        if (!Directory.Exists(DataStoreDirectory))
            Directory.CreateDirectory(DataStoreDirectory);
    }
    
    public bool ExistsInStore(string key) => File.Exists(DataStoreDirectory + key);

    public bool WriteToStore(string key, byte[] data)
    {
        try
        {
            File.WriteAllBytes(DataStoreDirectory + key, data);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public byte[] GetDataFromStore(string key) => File.ReadAllBytes(DataStoreDirectory + key);
}