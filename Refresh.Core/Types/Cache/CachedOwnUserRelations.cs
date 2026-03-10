using Refresh.Core.Types.Relations;

namespace Refresh.Core.Types.Cache;

public class CachedOwnUserRelations
{
    public OwnUserRelations Relations { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
}