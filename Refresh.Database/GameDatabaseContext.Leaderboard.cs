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
        .Include(s => s.Publisher)
        .Include(s => s.Level)
        .Include(s => s.Level.Publisher);
    
    public GameScore SubmitScore(ISerializedScore score, Token token, GameLevel level, IList<GameUser> players)
        => this.SubmitScore(score, token.User, level, token.TokenGame, token.TokenPlatform, players);

    public GameScore SubmitScore(ISerializedScore score, GameUser user, GameLevel level, TokenGame game, TokenPlatform platform, IList<GameUser> players)
    {
        // Throw incase the method directly gets called like this in a test
        if (players.Count <= 0)
        {
            throw new ArgumentException("Player list is empty!", nameof(players));
        }

        IEnumerable<ObjectId> playerIds = players.Select(u => u.UserId);

        GameScore newScore = new()
        {
            Score = score.Score,
            ScoreType = score.ScoreType,
            Level = level,
            PlayerIdsRaw = playerIds.Select(p => p.ToString()).ToList(),
            Publisher = user,
            ScoreSubmitted = this._time.Now,
            Game = game,
            Platform = platform,
        };

        GameScore? currentFirstPlace = this.GameScores
            .Where(s => s.LevelId == level.LevelId && s.ScoreType == score.ScoreType)
            .OrderByDescending(s => s.Score)
            .FirstOrDefault();

        // If the current first score is not 0, is lower than the new score and by a different first player,
        // show the overtake notification. This way the #1 player will not spam the #2 player by repeatedly improving their own score.
        bool showOvertakeNotification = currentFirstPlace != null
            && currentFirstPlace.Score > 0
            && currentFirstPlace.Score < score.Score 
            && currentFirstPlace.PublisherId != user.UserId;

        this.Write(() =>
        {
            this.GameScores.Add(newScore);
        });

        this.CreateEvent(newScore, new()
        {
            EventType = EventType.LevelScore,
            OverType = EventOverType.Activity,
            Actor = user,
        });

        // Notify the last #1 users that they've been overtaken
        // Only do this part of notifying after actually adding the new score to the database incase that fails
        // NOTE: If you want to change the notif text, make sure to adjust the respective Assert in 
        // ScoreLeaderboardTests.OnlySendOvertakeNotifsToRelevantPlayers() aswell!
        if (showOvertakeNotification)
        {
            // Below lines format the shown usernames to look like this: "UserA, UserB, UserC and UserD"
            IEnumerable<string> usernames = players.Select(u => u.Username);

            // players.Count is guaranteed to be equal to usernames.Count(), both are guaranteed to be > 0
            string usernamesToShow = players.Count > 1 
                ? $"{string.Join(", ", usernames.SkipLast(1))} and {usernames.Last()}"
                : usernames.First();

            // Don't notify users who have participated in both the overtaken and the new score
            IEnumerable<GameUser> usersToNotify = this.GetPlayersFromScore(currentFirstPlace!)
                .Where(p => !playerIds.Contains(p.UserId))
                .ToArray();
            
            foreach (GameUser player in usersToNotify)
            {
                this.AddNotification("Score overtaken", 
                    $"Your #1 score on {level.Title} has been overtaken by {usernamesToShow} in {score.ScoreType}-player-mode!", 
                    player, "medal");   
            }
        }

        return newScore;
    }

    /// <param name="scoreType">0 = don't filter by type</param>
    public DatabaseScoreList GetTopScoresForLevel(GameLevel level, int count, int skip, byte scoreType, bool showDuplicates = false, DateTimeOffset? minAge = null, GameUser? user = null)
    {
        IEnumerable<GameScore> scores = this.GameScoresIncluded
            .Where(s => s.LevelId == level.LevelId)
            .OrderByDescending(s => s.Score);
        
        if (scoreType != 0)
            scores = scores.Where(s => s.ScoreType == scoreType);

        if (!showDuplicates)
            scores = scores.DistinctBy(s => s.PublisherId);
        
        if (minAge != null)
            scores = scores.Where(s => s.ScoreSubmitted >= minAge);

        return new(scores.ToArray().Select((s, i) => new ScoreWithRank(s, i + 1)), skip, count, user);
    }

    public DatabaseScoreList GetRankedScoresAroundScore(GameScore score, int count, GameUser? user = null)
    {
        if (count % 2 != 1) throw new ArgumentException("The number of scores must be odd.", nameof(count));
        
        // this is probably REALLY fucking slow, and i probably shouldn't be trusted with LINQ anymore

        List<GameScore> scores = this.GameScoresIncluded
            .Where(s => s.ScoreType == score.ScoreType && s.LevelId == score.LevelId)
            .OrderByDescending(s => s.Score)
            .ToArray()
            .DistinctBy(s => s.PublisherId)
            .ToList();

        return new
        (
            scores.Select((s, i) => new ScoreWithRank(s, i + 1)),
            Math.Min(scores.Count, scores.IndexOf(score) - count / 2), // center user's score around other scores
            count, user
        );
    }
    
    /// <param name="scoreType">0 = don't filter by type</param>
    public DatabaseScoreList GetLevelTopScoresByFriends(GameUser user, GameLevel level, int skip, int count, byte scoreType, DateTimeOffset? minAge = null)
    {
        IEnumerable<ObjectId> mutuals = this.GetUsersMutuals(user)
            .Select(u => u.UserId)
            .Append(user.UserId);

        IEnumerable<GameScore> scores = this.GameScoresIncluded
            .Where(s => s.LevelId == level.LevelId)
            .OrderByDescending(s => s.Score)
            .ToArray()
            .DistinctBy(s => s.PublisherId)
            //TODO: THIS CALL IS EXTREMELY INEFFECIENT!!! once we are in postgres land, figure out a way to do this effeciently
            .Where(s => s.PlayerIds.Any(p => mutuals.Contains(p)));
        
        if (scoreType != 0)
            scores = scores.Where(s => s.ScoreType == scoreType);
        
        if (minAge != null)
            scores = scores.Where(s => s.ScoreSubmitted >= minAge);

        return new(scores.Select((s, i) => new ScoreWithRank(s, i + 1)), skip, count, user);
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