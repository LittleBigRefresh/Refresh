namespace Refresh.HttpServer.Storage;

public interface IDataStore
{
    bool ExistsInStore(string key);
    bool WriteToStore(string key, byte[] data);
    byte[] GetDataFromStore(string key);

    bool TryGetDataFromStore(string key, out byte[]? data)
    {
        try
        {
            if (this.ExistsInStore(key))
            {
                data = this.GetDataFromStore(key);
                return true;
            }
        }
        catch
        {
            data = null;
            return false;
        }

        data = null;
        return false;
    }
}