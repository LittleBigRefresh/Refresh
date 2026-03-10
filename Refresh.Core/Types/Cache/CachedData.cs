namespace Refresh.Core.Types.Cache;

#nullable disable

// TODO: use this to cache various data
public class CachedData<TCachedData>
{
    public TCachedData Cached { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}