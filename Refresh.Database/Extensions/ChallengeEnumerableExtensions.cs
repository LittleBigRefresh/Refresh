using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Challenges;

namespace Refresh.Database.Extensions;

public static class ChallengeEnumerableExtensions
{
    public static IEnumerable<GameChallenge> FilterByLevel(this IEnumerable<GameChallenge> challenges, GameLevel? level)
    {
        if (level != null) return challenges.Where(c => c.LevelId == level.LevelId);
        return challenges;
    }
}