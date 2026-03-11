namespace Refresh.Core.Types.Cache;

// TODO: use this to cache various data
public class CachedData<TData>
{
    public TData? Cached { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}