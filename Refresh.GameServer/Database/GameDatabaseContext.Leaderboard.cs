using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Leaderboard
{
    public bool SubmitScore(GameScore score, GameUser user, GameLevel level)
    {
        GameSubmittedScore newScore = new()
        {
            Score = score.Score,
            ScoreType = score.ScoreType,
            Level = level,
            Players = { user },
            ScoreSubmitted = DateTimeOffset.Now,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(newScore);
        });

        return true;
    }
    
    [UsedImplicitly] private record ScoreWithPlayer(GameLevel level, GameUser player);

    public IEnumerable<GameSubmittedScore> GetTopScoresForLevel(GameLevel level, int count, int skip, bool showDuplicates = false)
    {
        IEnumerable<GameSubmittedScore> scores = this._realm.All<GameSubmittedScore>()
            .Where(s => s.Level == level)
            .OrderByDescending(s => s.Score)
            .AsEnumerable();

        if (showDuplicates)
            scores = scores.DistinctBy(s => new ScoreWithPlayer(s.Level, s.Players[0]));

        return scores.Skip(skip).Take(count);
    }
    
    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameSubmittedScore? GetScoreByUuid(string? uuid)
    {
        if (uuid == null) return null;
        if(!ObjectId.TryParse(uuid, out ObjectId objectId)) return null;
        return this._realm.All<GameSubmittedScore>().FirstOrDefault(u => u.ScoreId == objectId);
    }
}