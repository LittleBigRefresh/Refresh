using Bunkum.Core.Storage;
using JetBrains.Annotations;
#pragma warning disable CS0162 // Unreachable code detected

namespace Refresh.Core.Storage;

/// <summary>
/// A datastore that uses production Refresh as a data source.
/// Intended for testing purposes only; performance is not considered.
/// </summary>
public class RemoteRefreshDataStore : IDataStore
{
    private const string SourceUrl = "https://lbp.lbpbonsai.com/api/v3/"; 
    private const string WriteError = $"{nameof(RemoteRefreshDataStore)} is a read-only source, and cannot be written to.";
    private const bool HideImages = true;
    
    private readonly HttpClient _client = new()
    {
        BaseAddress = new Uri(SourceUrl),
    };
    
    [Pure]
    private string? GetPath(ReadOnlySpan<char> key)
    {
        if (key.StartsWith("png/"))
            return $"assets/{key[4..]}/image";

        return $"assets/{key}/download";
    }

    private HttpResponseMessage Get(string endpoint)
    {
        HttpResponseMessage message = this._client.GetAsync(endpoint).Result;
        return message;
    }
    
    public bool ExistsInStore(string key)
    {
        if (HideImages && key.StartsWith("png/"))
            return false;

        string? path = this.GetPath(key);

        if (path == null)
            throw new FormatException("The key was invalid.");
        
        HttpResponseMessage resp = this.Get(path);
        return resp.IsSuccessStatusCode;
    }

    public bool WriteToStore(string key, byte[] data) => throw new InvalidOperationException(WriteError);

    public byte[] GetDataFromStore(string key)
    {
        string? path = this.GetPath(key);
        
        if (path == null)
            throw new FormatException("The key was invalid.");

        HttpResponseMessage resp = this.Get(path);
        resp.EnsureSuccessStatusCode();
        
        return resp.Content.ReadAsByteArrayAsync().Result;
    }
    
    public bool RemoveFromStore(string key) => throw new InvalidOperationException(WriteError);
    public string[] GetKeysFromStore() => []; // TODO: Implement when we want to store these as GameAssets
    public bool WriteToStoreFromStream(string key, Stream data) => throw new InvalidOperationException(WriteError);
    public Stream GetStreamFromStore(string key) => new MemoryStream(this.GetDataFromStore(key));
    public Stream OpenWriteStream(string key) => throw new InvalidOperationException(WriteError);
}