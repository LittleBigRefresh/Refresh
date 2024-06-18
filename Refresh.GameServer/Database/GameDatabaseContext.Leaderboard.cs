using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Leaderboard
{
    public GameSubmittedScore SubmitScore(SerializedScore score, Token token, GameLevel level)
        => this.SubmitScore(score, token.User, level, token.TokenGame, token.TokenPlatform);

    public GameSubmittedScore SubmitScore(SerializedScore score, GameUser user, GameLevel level, TokenGame game, TokenPlatform platform)
    {
        GameSubmittedScore newScore = new()
        {
            Score = score.Score,
            ScoreType = score.ScoreType,
            Level = level,
            Players = { user },
            ScoreSubmitted = this._time.Now,
            Game = game,
            Platform = platform,
        };

        this.Write(() =>
        {
            this.GameSubmittedScores.Add(newScore);
        });

        this.CreateLevelScoreEvent(user, newScore);

        #region Notifications
        
        IEnumerable<ScoreWithRank> rankedScores = GetRankedScoresAroundScore(newScore, 3).ToList();
        ScoreWithRank? rankOne = rankedScores.FirstOrDefault(s => s.rank == 1);
        ScoreWithRank? rankTwo = rankedScores.FirstOrDefault(s => s.rank == 2);
        if (rankOne != null && rankTwo != null &&
            rankOne.score.ScoreId == newScore.ScoreId && // if submitted score is the new #1
            rankTwo.score.Score > 0 // don't send notification if the last #1 score was just 0
           )
        {
            // Notify the last #1 users that they've been overtaken
            foreach (GameUser? player in rankTwo.score.Players)
            {
                if (player == null)
                    continue;
                
                this.AddNotification("Score overtaken", 
                    $"Your #1 score on {level.Title} has been overtaken by {user.Username}!", 
                    player, "medal");   
            }
        }
        
        #endregion

        return newScore;
    }
    
    [UsedImplicitly] private record ScoreLevelWithPlayer(GameLevel Level, GameUser Player);

    public DatabaseList<GameSubmittedScore> GetTopScoresForLevel(GameLevel level, int count, int skip, byte type, bool showDuplicates = false)
    {
        IEnumerable<GameSubmittedScore> scores = this.GameSubmittedScores
            .Where(s => s.ScoreType == type && s.Level == level)
            .OrderByDescending(s => s.Score)
            .AsEnumerable();

        if (!showDuplicates)
            scores = scores.DistinctBy(s => new ScoreLevelWithPlayer(s.Level, s.Players[0]));

        return new DatabaseList<GameSubmittedScore>(scores, skip, count);
    }

    public IEnumerable<ScoreWithRank> GetRankedScoresAroundScore(GameSubmittedScore score, int count)
    {
        if (count % 2 != 1) throw new ArgumentException("The number of scores must be odd.", nameof(count));
        
        // this is probably REALLY fucking slow, and i probably shouldn't be trusted with LINQ anymore

        List<GameSubmittedScore> scores = this.GameSubmittedScores.Where(s => s.ScoreType == score.ScoreType && s.Level == score.Level)
            .OrderByDescending(s => s.Score)
            .AsEnumerable()
            .ToList();

        scores = scores.DistinctBy(s => new ScoreLevelWithPlayer(s.Level, s.Players[0]))
            .ToList();

        return scores.Select((s, i) => new ScoreWithRank(s, i + 1))
            .Skip(Math.Min(scores.Count, scores.IndexOf(score) - count / 2)) // center user's score around other scores
            .Take(count);
    }

    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameSubmittedScore? GetScoreByUuid(string? uuid)
    {
        if (uuid == null) return null;
        if(!ObjectId.TryParse(uuid, out ObjectId objectId)) return null;
        return this.GameSubmittedScores.FirstOrDefault(u => u.ScoreId == objectId);
    }
    
    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameSubmittedScore? GetScoreByObjectId(ObjectId? id)
    {
        if (id == null) return null;
        return this.GameSubmittedScores.FirstOrDefault(u => u.ScoreId == id);
    }
    
    public void DeleteScore(GameSubmittedScore score)
    {
        IQueryable<Event> scoreEvents = this.Events
            .Where(e => e._StoredDataType == (int)EventDataType.Score && e.StoredObjectId == score.ScoreId);
        
        this.Write(() =>
        {
            this._realm.RemoveRange(scoreEvents);
            this._realm.Remove(score);
        });
    }
    
    public void DeleteScoresSetByUser(GameUser user)
    {
        IEnumerable<GameSubmittedScore> scores = this.GameSubmittedScores
            // FIXME: Realm (ahem, I mean the atlas device sdk *rolls eyes*) is a fucking joke.
            // Realm doesn't support .Contains on IList<T>. Yes, really.
            // This means we are forced to iterate over EVERY SCORE.
            // I can't wait for Postgres.
            .AsEnumerable()
            .Where(s => s.Players.Contains(user));
        
        this.Write(() =>
        {
            foreach (GameSubmittedScore score in scores)
            {
                IQueryable<Event> scoreEvents = this.Events
                    .Where(e => e._StoredDataType == (int)EventDataType.Score && e.StoredObjectId == score.ScoreId);
                
                this._realm.RemoveRange(scoreEvents);
                this._realm.Remove(score);
            }
        });
    }
}