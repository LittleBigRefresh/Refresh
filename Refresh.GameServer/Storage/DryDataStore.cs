using System.Text;
using Bunkum.Core.Storage;
using Refresh.GameServer.Verification;

namespace Refresh.GameServer.Storage;

public class DryDataStore : IDataStore
{
    private DryArchiveConfig _config;
    
    public DryDataStore(DryArchiveConfig config)
    {
        this._config = config;
    }
    
    private string? GetZipPath(string hash)
    {
        if (!CommonPatterns.Sha1Regex().IsMatch(hash))
            return null;
        
        StringBuilder builder = new();
        builder.Append(this._config.Location);
        
        if (this._config.UseFolderNames)
        {
            builder.Append("dry23r");
            builder.Append(hash[0]);
            builder.Append('/');
        }
        
        builder.Append("dry");
        builder.Append(hash.AsSpan(0, 2));
        builder.Append(".zip");
        
        return builder.ToString();
    }
    
    public bool ExistsInStore(string key)
    {
        Console.WriteLine(this.GetZipPath(key));
        throw new NotImplementedException();
    }
    
    public bool WriteToStore(string key, byte[] data)
    {
        throw new NotImplementedException();
    }
    
    public byte[] GetDataFromStore(string key)
    {
        throw new NotImplementedException();
    }
    
    public bool RemoveFromStore(string key)
    {
        throw new NotImplementedException();
    }
    
    public string[] GetKeysFromStore()
    {
        throw new NotImplementedException();
    }
    
    public bool WriteToStoreFromStream(string key, Stream data)
    {
        throw new NotImplementedException();
    }
    
    public Stream GetStreamFromStore(string key)
    {
        throw new NotImplementedException();
    }
    
    public Stream OpenWriteStream(string key)
    {
        throw new NotImplementedException();
    }
}