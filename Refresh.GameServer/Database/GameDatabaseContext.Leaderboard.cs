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

    public IEnumerable<GameSubmittedScore> GetTopScoresForLevel(GameLevel level, int count, int skip)
    {
        return this._realm.All<GameSubmittedScore>()
            .Where(s => s.Level == level)
            .OrderByDescending(s => s.Score)
            .AsEnumerable()
            .Skip(skip)
            .Take(count);
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