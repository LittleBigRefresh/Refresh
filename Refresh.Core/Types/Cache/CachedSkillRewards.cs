using Refresh.Database.Models.Levels;

namespace Refresh.Core.Types.Cache;

public class CachedSkillRewards
{
    public List<GameSkillReward> SkillRewards { get; set; } = [];
    public DateTimeOffset ExpiresAt { get; set; }
}