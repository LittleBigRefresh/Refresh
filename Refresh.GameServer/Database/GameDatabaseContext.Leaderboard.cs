using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Leaderboard
{
    public GameSubmittedScore? SubmitScore(GameScore score, GameUser user, GameLevel level)
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

        return newScore;
    }
    
    [UsedImplicitly] private record ScoreLevelWithPlayer(GameLevel level, GameUser player);

    public IEnumerable<GameSubmittedScore> GetTopScoresForLevel(GameLevel level, int count, int skip, bool showDuplicates = false)
    {
        IEnumerable<GameSubmittedScore> scores = this._realm.All<GameSubmittedScore>()
            .Where(s => s.Level == level)
            .OrderByDescending(s => s.Score)
            .AsEnumerable();

        if (!showDuplicates)
            scores = scores.DistinctBy(s => new ScoreLevelWithPlayer(s.Level, s.Players[0]));

        return scores.Skip(skip).Take(count);
    }
    

    public IEnumerable<ScoreWithRank>? GetRankedScoresAroundScore(GameSubmittedScore score, int count)
    {
        if (count % 2 != 1) throw new InvalidOperationException("Count must be odd!");
        
        // this is probably REALLY fucking slow, and i probably shouldn't be trusted with LINQ anymore

        List<GameSubmittedScore> scores = this._realm.All<GameSubmittedScore>()
            .Where(s => s.Level == score.Level)
            .OrderByDescending(s => s.Score)
            .AsEnumerable()
            .ToList();

        if (!scores.Contains(score)) return null;

        scores = scores.DistinctBy(s => new ScoreLevelWithPlayer(s.Level, s.Players[0]))
            .ToList();

        return scores.Select((s, i) => new ScoreWithRank(s, i))
            .Skip(Math.Min(scores.Count, scores.IndexOf(score) - count / 2)) // center user's score around other scores
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