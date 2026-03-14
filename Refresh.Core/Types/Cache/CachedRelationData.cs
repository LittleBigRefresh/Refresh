namespace Refresh.Core.Types.Cache;

public class CachedRelationData<TSourceKey, TTargetKey, TData>
{
    public TSourceKey SourceKey { get; set; }
    public TTargetKey TargetKey { get; set; }
    public TData Content { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public CachedRelationData(TSourceKey sourceKey, TTargetKey targetKey, TData content, DateTimeOffset expiresAt)
    {
        SourceKey = sourceKey;
        TargetKey = targetKey;
        Content = content;
        ExpiresAt = expiresAt;
    }
}