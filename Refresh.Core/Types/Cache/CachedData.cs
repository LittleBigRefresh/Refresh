namespace Refresh.Core.Types.Cache;

public class CachedData<TFirstKey, TData>
{
    public TFirstKey Key { get; set; }
    public TData Content { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public CachedData(TFirstKey key, TData content, DateTimeOffset expiresAt)
    {
        Key = key;
        Content = content;
        ExpiresAt = expiresAt;
    }
}