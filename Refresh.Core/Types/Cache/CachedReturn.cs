namespace Refresh.Core.Types.Cache;

public class CachedReturn<TData>
{
    public TData Content { get; set; }
    public bool WasRefreshed { get; set; }

    public CachedReturn(TData content, bool wasRefreshed)
    {
        Content = content;
        WasRefreshed = wasRefreshed;
    }
}