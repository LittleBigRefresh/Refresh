namespace Refresh.Core.Types.Cache;

public class CachedData<TData>
{
    public TData Content { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public CachedData(TData content, DateTimeOffset expiresAt)
    {
        Content = content;
        ExpiresAt = expiresAt;
    }
}