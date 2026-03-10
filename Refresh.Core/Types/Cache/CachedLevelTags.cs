using Refresh.Database.Models.Levels;

namespace Refresh.Core.Types.Cache;

public class CachedLevelTags
{
    public List<Tag> Tags { get; set; } = [];
    public DateTimeOffset ExpiresAt { get; set; }
}