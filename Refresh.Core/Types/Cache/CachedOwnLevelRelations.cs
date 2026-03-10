using Refresh.Core.Types.Relations;

namespace Refresh.Core.Types.Cache;

public class CachedOwnLevelRelations
{
    public OwnLevelRelations Relations { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
}