using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.Database.Query;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.Database;

public partial class GameDatabaseContext // Leaderboard
{
    private IQueryable<GameScore> GameScoresIncluded => this.GameScores
        .Include(s => s.Level)
        .Include(s => s.Level.Publisher);
    
    public GameScore SubmitScore(ISerializedScore score, Token token, GameLevel level)
        => this.SubmitScore(score, token.User, level, token.TokenGame, token.TokenPlatform);

    public GameScore SubmitScore(ISerializedScore score, GameUser user, GameLevel level, TokenGame game, TokenPlatform platform)
    {
        GameScore newScore = new()
        {
            Score = score.Score,
            ScoreType = score.ScoreType,
            Level = level,
            PlayerIdsRaw = [ user.UserId.ToString() ],
            ScoreSubmitted = this._time.Now,
            Game = game,
            Platform = platform,
        };

        this.Write(() =>
        {
            this.GameScores.Add(newScore);
        });

        this.CreateLevelScoreEvent(user, newScore);

        #region Notifications
        
        IEnumerable<ScoreWithRank> rankedScores = GetRankedScoresAroundScore(newScore, 3).Items;
        ScoreWithRank? rankOne = rankedScores.FirstOrDefault(s => s.rank == 1);
        ScoreWithRank? rankTwo = rankedScores.FirstOrDefault(s => s.rank == 2);
        if (rankOne != null && rankTwo != null &&
            rankOne.score.ScoreId == newScore.ScoreId && // if submitted score is the new #1
            rankTwo.score.Score > 0 // don't send notification if the last #1 score was just 0
           )
        {
            // Notify the last #1 users that they've been overtaken
            foreach (GameUser player in this.GetPlayersFromScore(rankTwo.score).ToArray())
            {
                this.AddNotification("Score overtaken", 
                    $"Your #1 score on {level.Title} has been overtaken by {user.Username}!", 
                    player, "medal");   
            }
        }
        
        #endregion

        return newScore;
    }
    
    public DatabaseList<GameScore> GetTopScoresForLevel(GameLevel level, int count, int skip, byte type, bool showDuplicates = false)
    {
        IEnumerable<GameScore> scores = this.GameScoresIncluded
            .Where(s => s.ScoreType == type && s.LevelId == level.LevelId)
            .OrderByDescending(s => s.Score);

        if (!showDuplicates)
            scores = scores.DistinctBy(s => s.PlayerIds[0]);

        return new DatabaseList<GameScore>(scores, skip, count);
    }

    public DatabaseList<ScoreWithRank> GetRankedScoresAroundScore(GameScore score, int count)
    {
        if (count % 2 != 1) throw new ArgumentException("The number of scores must be odd.", nameof(count));
        
        // this is probably REALLY fucking slow, and i probably shouldn't be trusted with LINQ anymore

        List<GameScore> scores = this.GameScoresIncluded
            .Where(s => s.ScoreType == score.ScoreType && s.LevelId == score.LevelId)
            .OrderByDescending(s => s.Score)
            .ToArray()
            .DistinctBy(s => s.PlayerIds[0])
            .ToList();

        return new
        (
            scores.Select((s, i) => new ScoreWithRank(s, i + 1)),
            Math.Min(scores.Count, scores.IndexOf(score) - count / 2), // center user's score around other scores
            count
        );
    }
    
    public DatabaseList<ScoreWithRank> GetLevelTopScoresByFriends(GameUser user, GameLevel level, int count, byte scoreType)
    {
        IEnumerable<ObjectId> mutuals = this.GetUsersMutuals(user)
            .Select(u => u.UserId)
            .Append(user.UserId);

        IEnumerable<GameScore> scores = this.GameScoresIncluded
            .Where(s => s.ScoreType == scoreType && s.LevelId == level.LevelId)
            .OrderByDescending(s => s.Score)
            .ToArray()
            .DistinctBy(s => s.PlayerIds[0])
            //TODO: THIS CALL IS EXTREMELY INEFFECIENT!!! once we are in postgres land, figure out a way to do this effeciently
            .Where(s => s.PlayerIds.Any(p => mutuals.Contains(p)));

        return new(scores.Select((s, i) => new ScoreWithRank(s, i + 1)), 0, count);
    }

    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameScore? GetScoreByUuid(string? uuid)
    {
        if (uuid == null) return null;
        if(!ObjectId.TryParse(uuid, out ObjectId objectId)) return null;

        return GetScoreByObjectId(objectId);
    }
    
    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameScore? GetScoreByObjectId(ObjectId? id)
    {
        if (id == null) return null;
        return this.GameScoresIncluded
            .FirstOrDefault(u => u.ScoreId == id);
    }
    
    public void DeleteScore(GameScore score)
    {
        IQueryable<Event> scoreEvents = this.Events
            .Where(e => e.StoredDataType == EventDataType.Score && e.StoredObjectId == score.ScoreId);
        
        this.Write(() =>
        {
            this.Events.RemoveRange(scoreEvents);
            this.GameScores.Remove(score);
        });
    }
    
    public void DeleteScoresSetByUser(GameUser user)
    {
        IEnumerable<GameScore> scores = this.GameScores
            // FIXME: Realm (ahem, I mean the atlas device sdk *rolls eyes*) is a fucking joke.
            // Realm doesn't support .Contains on IList<T>. Yes, really.
            // This means we are forced to iterate over EVERY SCORE.
            // I can't wait for Postgres.
            .AsEnumerableIfRealm()
            .Where(s => s.PlayerIdsRaw.Contains(user.UserId.ToString()))
            .ToArrayIfPostgres();
        
        this.Write(() =>
        {
            foreach (GameScore score in scores)
            {
                IQueryable<Event> scoreEvents = this.Events
                    .Where(e => e.StoredDataType == EventDataType.Score && e.StoredObjectId == score.ScoreId);
                
                this.Events.RemoveRange(scoreEvents);
                this.GameScores.Remove(score);
            }
        });
    }

    public IEnumerable<GameUser> GetPlayersFromScore(GameScore score)
    {
        IEnumerable<ObjectId> playerIds = score.PlayerIds.Select(p => p);
        return this.GameUsersIncluded
            .Where(u => playerIds.Contains(u.UserId));
    }
    
    public GameUser? GetSubmittingPlayerFromScore(GameScore score)
    {
        return this.GetUserByUuid(score.PlayerIdsRaw.FirstOrDefault());
    }
}