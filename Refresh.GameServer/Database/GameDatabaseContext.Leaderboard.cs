using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Leaderboard
{
    public GameSubmittedScore SubmitUserLevelScore(SerializedScore score, GameUser user, GameLevel level, TokenGame game)
    {
        GameSubmittedScore newScore = new()
        {
            Score = score.Score,
            ScoreType = score.ScoreType,
            Level = level,
            Players = { user },
            ScoreSubmitted = this._time.Now,
            DeveloperId = -1,
            LevelType = GameSubmittedScoreLevelType.User,
            Game = game,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(newScore);
        });

        this.CreateSubmittedScoreCreateEvent(user, newScore);

        return newScore;
    }
    
    public GameSubmittedScore SubmitDeveloperLevelScore(SerializedScore score, GameUser user, int developerId, TokenGame game)
    {
        GameSubmittedScore newScore = new()
        {
            Score = score.Score,
            ScoreType = score.ScoreType,
            Level = null,
            Players = { user },
            ScoreSubmitted = this._time.Now,
            DeveloperId = developerId,
            LevelType = GameSubmittedScoreLevelType.Developer,
            Game = game,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(newScore);
        });

        this.CreateSubmittedScoreCreateEvent(user, newScore);

        return newScore;
    }
    
    [UsedImplicitly] private record ScoreLevelWithPlayer(GameLevel? Level, int? DeveloperId, GameUser Player);

    public DatabaseList<GameSubmittedScore> GetTopScoresForLevel(GameLevel? level, int? developerId, TokenGame game, int count, int skip, byte type, bool showDuplicates = false)
    {
        //Asset that exactly one of the fields are set
        if (!(level != null ^ developerId != null))
            throw new ArgumentException($"Only {nameof(level)} *or* {nameof(developerId)} can be set at once!");

        IEnumerable<GameSubmittedScore> scores = (developerId.HasValue
                ? this._realm.All<GameSubmittedScore>().Where(s => s.ScoreType == type && s._Game == (int)game && s.DeveloperId == developerId.Value)
                : this._realm.All<GameSubmittedScore>().Where(s => s.ScoreType == type && s.Level == level))
            .AsEnumerable();

        if (!showDuplicates)
            scores = scores.DistinctBy(s => new ScoreLevelWithPlayer(s.Level, s.DeveloperId, s.Players[0]));

        return new DatabaseList<GameSubmittedScore>(scores, skip, count);
    }

    public IEnumerable<ScoreWithRank> GetRankedScoresAroundScore(GameSubmittedScore score, int count)
    {
        if (count % 2 != 1) throw new ArgumentException("The number of scores must be odd.", nameof(count));
        
        // this is probably REALLY fucking slow, and i probably shouldn't be trusted with LINQ anymore
        
        // List<GameSubmittedScore> scores = this._realm.All<GameSubmittedScore>()
        //     .Where(s => s.ScoreType == score.ScoreType
        //              && score._LevelType == (int)GameSubmittedScoreLevelType.User
        //         ? (s.Level == score.Level)
        //         : (s._Game == score._Game && s.DeveloperId == score.DeveloperId))
        //     .OrderByDescending(s => s.Score)
        //     .AsEnumerable()
        //     .ToList();

        List<GameSubmittedScore> scores = (score.LevelType == GameSubmittedScoreLevelType.User
            ? this._realm.All<GameSubmittedScore>().Where(s => s.ScoreType == score.ScoreType && s.Level == score.Level)
            : this._realm.All<GameSubmittedScore>().Where(s => s.ScoreType == score.ScoreType && s._Game == score._Game && s.DeveloperId == score.DeveloperId))
            .OrderByDescending(s => s.Score)
            .AsEnumerable()
            .ToList();

        scores = scores.DistinctBy(s => new ScoreLevelWithPlayer(s.Level, s.DeveloperId, s.Players[0]))
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
    
    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameSubmittedScore? GetScoreByObjectId(ObjectId? id)
    {
        if (id == null) return null;
        return this._realm.All<GameSubmittedScore>().FirstOrDefault(u => u.ScoreId == id);
    }
}