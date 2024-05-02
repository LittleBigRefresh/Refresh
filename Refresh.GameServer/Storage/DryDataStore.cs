using System.IO.Compression;
using System.Text;
using Bunkum.Core.Storage;
using JetBrains.Annotations;
using Refresh.GameServer.Verification;

namespace Refresh.GameServer.Storage;

public class DryDataStore : IDataStore
{
    private readonly DryArchiveConfig _config;
    
    private const string WriteError = $"{nameof(DryDataStore)} is an archive, and cannot be written to.";
    
    public DryDataStore(DryArchiveConfig config)
    {
        this._config = config;
    }
    
    private string? GetZipPath(ReadOnlySpan<char> hash)
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
        builder.Append(hash.Slice(0, 2));
        builder.Append(".zip");
        
        return builder.ToString();
    }
    
    private static string GetEntryPath(ReadOnlySpan<char> hash)
    {
        StringBuilder builder = new();
        builder.Append(hash.Slice(0, 2));
        builder.Append('/');
        builder.Append(hash.Slice(2, 2));
        builder.Append('/');
        builder.Append(hash);
        
        return builder.ToString();
    }
    
    [Pure]
    private ZipArchive? OpenZipForHash(string hash)
    {
        string? zipPath = this.GetZipPath(hash);
        if (zipPath == null) return null;
        
        FileStream stream = File.OpenRead(zipPath);
        ZipArchive zipArchive = new(stream, ZipArchiveMode.Read, false);
        
        return zipArchive;
    }
    
    public bool ExistsInStore(string key)
    {
        using ZipArchive? zip = this.OpenZipForHash(key);
        return zip?.GetEntry(GetEntryPath(key)) != null;
    }
    
    public bool WriteToStore(string key, byte[] data)
    {
        throw new InvalidOperationException(WriteError);
    }
    
    public byte[] GetDataFromStore(string key)
    {
        throw new NotImplementedException();
    }
    
    public bool RemoveFromStore(string key)
    {
        throw new InvalidOperationException(WriteError);
    }
    
    public string[] GetKeysFromStore()
    {
        throw new NotImplementedException();
    }
    
    public bool WriteToStoreFromStream(string key, Stream data)
    {
        throw new InvalidOperationException(WriteError);
    }
    
    public Stream GetStreamFromStore(string key)
    {
        throw new NotImplementedException();
    }
    
    public Stream OpenWriteStream(string key)
    {
        throw new InvalidOperationException(WriteError);
    }
}