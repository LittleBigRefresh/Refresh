using Refresh.Database.Models.Levels.Challenges;

namespace Refresh.Database.Query;

public interface ICreateChallengeInfo
{
    string Name { get; }
    int StartCheckpointUid { get; }
    int FinishCheckpointUid { get; }
    long ExpiresAt { get; }
    IEnumerable<GameChallengeCriteriaType> CriteriaTypes { get; }
}