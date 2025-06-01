using System.Text;
using Bunkum.Core.Storage;
using JetBrains.Annotations;
using Refresh.Common.Verification;

namespace Refresh.Core.Storage;

public class DryDataStore : IDataStore
{
    private readonly DryArchiveConfig _config;
    
    private const string WriteError = $"{nameof(DryDataStore)} is an archive, and cannot be written to.";
    
    public DryDataStore(DryArchiveConfig config)
    {
        this._config = config;
    }
    
    [Pure]
    private string? GetPath(ReadOnlySpan<char> hash)
    {
        if (!CommonPatterns.Sha1Regex().IsMatch(hash))
            return null;
        
        StringBuilder builder = new();
        
        // /var/dry
        builder.Append(this._config.Location);
        
        // /var/dry/
        if (!this._config.Location.EndsWith('/'))
            builder.Append('/');
        
        // /var/dry/dry23r0/
        if (this._config.UseFolderNames)
        {
            builder.Append("dry23r");
            builder.Append(hash[0]);
            builder.Append('/');
        }
        
        // /var/dry/dry23r0/01/
        builder.Append(hash.Slice(0, 2));
        builder.Append('/');
        
        // /var/dry/dry23r0/01/02/
        builder.Append(hash.Slice(2, 2));
        builder.Append('/');
        
        // /var/dry/dry2er0/01/02/010220123644c8e53d4054bf0d30d0e2bd0786ff8
        builder.Append(hash);
        
        return builder.ToString();
    }
    
    public bool ExistsInStore(string key) => File.Exists(this.GetPath(key));
    
    public bool WriteToStore(string key, byte[] data)
    {
        throw new InvalidOperationException(WriteError);
    }
    
    public byte[] GetDataFromStore(string key)
    {
        string? path = this.GetPath(key);
        
        if (path == null)
            throw new FormatException("The key was invalid.");
        
        return File.ReadAllBytes(path);
    }
    
    public bool RemoveFromStore(string key)
    {
        throw new InvalidOperationException(WriteError);
    }
    
    public string[] GetKeysFromStore() => []; // TODO: Implement when we want to store these as GameAssets
    
    public bool WriteToStoreFromStream(string key, Stream data)
    {
        throw new InvalidOperationException(WriteError);
    }
    
    public Stream GetStreamFromStore(string key)
    {
        string? path = this.GetPath(key);
        
        if (path == null)
            throw new FormatException("The key was invalid.");
        
        return File.OpenRead(path);
    }
    
    public Stream OpenWriteStream(string key)
    {
        throw new InvalidOperationException(WriteError);
    }
}