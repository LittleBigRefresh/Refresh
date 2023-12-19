using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Levels
{
    public void AddLevel(GameLevel level)
    {
        if (level.Publisher == null) throw new InvalidOperationException("Cannot create a level without a publisher");

        long timestamp = this._time.TimestampMilliseconds;
        this.AddSequentialObject(level, () =>
        {
            level.PublishDate = timestamp;
            level.UpdateDate = timestamp;
        });
    }

    public GameLevel GetStoryLevelById(int id)
    {
        GameLevel? level = this._realm.All<GameLevel>().FirstOrDefault(l => l.StoryId == id);

        if (level != null) return level;
        
        //Create a new level for the story level
        level = new()
        {
            Title = $"Story level #{id}",
            Publisher = null,
            Location = GameLocation.Zero,
            Source = GameLevelSource.Story,
            StoryId = id,
        };
            
        //Add the new story level to the database
        long timestamp = this._time.TimestampMilliseconds;
        this.AddSequentialObject(level, () =>
        {
            level.PublishDate = timestamp;
            level.UpdateDate = timestamp;
        });
        
        return level;
    }

    public GameLevel? UpdateLevel(GameLevel level, GameUser user)
    {
        // Verify if this level is able to be republished
        GameLevel? oldSlot = this.GetLevelById(level.LevelId);
        if (oldSlot == null) return null;
            
        Debug.Assert(oldSlot.Publisher != null);
        if (oldSlot.Publisher.UserId != user.UserId) return null;
        
        // All checks passed, lets move

        long oldDate = oldSlot.PublishDate;
        this._realm.Write(() =>
        {
            PropertyInfo[] userProps = typeof(GameLevel).GetProperties();
            foreach (PropertyInfo prop in userProps)
            {
                if (!prop.CanWrite || !prop.CanRead) continue;
                prop.SetValue(oldSlot, prop.GetValue(level));
            }

            oldSlot.Publisher = user;
            
            oldSlot.PublishDate = oldDate;
            oldSlot.UpdateDate = this._time.TimestampMilliseconds;
        });

        return oldSlot;
    }

    public GameLevel UpdateLevel(ApiEditLevelRequest body, GameLevel level)
    {
        this._realm.Write(() =>
        {
            PropertyInfo[] userProps = typeof(ApiEditLevelRequest).GetProperties();
            foreach (PropertyInfo prop in userProps)
            {
                if (!prop.CanWrite || !prop.CanRead) continue;
                
                object? propValue = prop.GetValue(body);
                if(propValue == null) continue;

                PropertyInfo? gameLevelProp = level.GetType().GetProperty(prop.Name);
                Debug.Assert(gameLevelProp != null, $"Invalid property {prop.Name} on {nameof(ApiEditLevelRequest)}");
                
                 gameLevelProp.SetValue(level, prop.GetValue(body));
            }
            
            level.UpdateDate = this._time.TimestampMilliseconds;
        });

        return level;
    }

    public void DeleteLevel(GameLevel level)
    {
        this._realm.Write(() =>
        {
            IQueryable<Event> levelEvents = this._realm.All<Event>()
                .Where(e => e._StoredDataType == (int)EventDataType.Level && e.StoredSequentialId == level.LevelId);
            
            this._realm.RemoveRange(levelEvents);

            #region This is so terrible it needs to be hidden away

            IQueryable<FavouriteLevelRelation> favouriteLevelRelations = this._realm.All<FavouriteLevelRelation>().Where(r => r.Level == level);
            this._realm.RemoveRange(favouriteLevelRelations);
            
            IQueryable<PlayLevelRelation> playLevelRelations = this._realm.All<PlayLevelRelation>().Where(r => r.Level == level);
            this._realm.RemoveRange(playLevelRelations);
            
            IQueryable<QueueLevelRelation> queueLevelRelations = this._realm.All<QueueLevelRelation>().Where(r => r.Level == level);
            this._realm.RemoveRange(queueLevelRelations);
            
            IQueryable<RateLevelRelation> rateLevelRelations = this._realm.All<RateLevelRelation>().Where(r => r.Level == level);
            this._realm.RemoveRange(rateLevelRelations);
            
            IQueryable<UniquePlayLevelRelation> uniquePlayLevelRelations = this._realm.All<UniquePlayLevelRelation>().Where(r => r.Level == level);
            this._realm.RemoveRange(uniquePlayLevelRelations);
            
            IQueryable<GameSubmittedScore> scores = this._realm.All<GameSubmittedScore>().Where(r => r.Level == level);
            
            foreach (GameSubmittedScore score in scores)
            {
                IQueryable<Event> scoreEvents = this._realm.All<Event>()
                    .Where(e => e._StoredDataType == (int)EventDataType.Score && e.StoredObjectId == score.ScoreId);
                this._realm.RemoveRange(scoreEvents);
            }
            
            this._realm.RemoveRange(scores);

            #endregion
            
        });

        //do in separate transaction in a vain attempt to fix Weirdness with favourite level relations having missing levels
        this._realm.Write(() =>
        {
            this._realm.Remove(level);
        });
    }

    private IQueryable<GameLevel> GetLevelsByGameVersion(TokenGame gameVersion) 
        => this._realm.All<GameLevel>().Where(l => l._Source == (int)GameLevelSource.User).FilterByGameVersion(gameVersion);

    [Pure]
    public DatabaseList<GameLevel> GetLevelsByUser(GameUser user, int count, int skip, LevelFilterSettings levelFilterSettings)
    {
        if (user.Username == DeletedUser.Username)
        {
            return new DatabaseList<GameLevel>(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion).FilterByLevelFilterSettings(null, levelFilterSettings).Where(l => l.Publisher == null), skip, count);
        }
        
        return new DatabaseList<GameLevel>(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion).FilterByLevelFilterSettings(null, levelFilterSettings).Where(l => l.Publisher == user), skip, count);
    }

    [Pure]
    public DatabaseList<GameLevel> GetAllUserLevels() 
        => new(this._realm.All<GameLevel>().Where(l => l._Source == (int)GameLevelSource.User));

    [Pure]
    public DatabaseList<GameLevel> GetNewestLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings) =>
        new(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .OrderByDescending(l => l.PublishDate), skip, count);
    
    [Pure]
    public DatabaseList<GameLevel> GetRandomLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings) =>
        new(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .AsEnumerable()
            .OrderBy(_ => Random.Shared.Next()), skip, count);
    
    // TODO: reduce code duplication for getting most of x
    [Pure]
    public DatabaseList<GameLevel> GetMostHeartedLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        IQueryable<FavouriteLevelRelation> favourites = this._realm.All<FavouriteLevelRelation>();
        
        IEnumerable<GameLevel> mostHeartedLevels = favourites
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Level)
            .Where(l => l != null)
            .Where(l => l._Source == (int)GameLevelSource.User)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .FilterByGameVersion(levelFilterSettings.GameVersion);

        return new DatabaseList<GameLevel>(mostHeartedLevels, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetMostUniquelyPlayedLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        IQueryable<UniquePlayLevelRelation> uniquePlays = this._realm.All<UniquePlayLevelRelation>();
        
        IEnumerable<GameLevel> mostPlayed = uniquePlays
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Level)
            .Where(l => l != null)
            .Where(l => l._Source == (int)GameLevelSource.User)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .FilterByGameVersion(levelFilterSettings.GameVersion);

        return new DatabaseList<GameLevel>(mostPlayed, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetMostReplayedLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        IQueryable<PlayLevelRelation> plays = this._realm.All<PlayLevelRelation>();
        
        IEnumerable<GameLevel> mostPlayed = plays
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Count = g.Sum(p => p.Count) })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Level)
            .Where(l => l != null)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .FilterByGameVersion(levelFilterSettings.GameVersion);

        return new DatabaseList<GameLevel>(mostPlayed, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetHighestRatedLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        IQueryable<RateLevelRelation> ratings = this._realm.All<RateLevelRelation>();
        
        IEnumerable<GameLevel> highestRated = ratings
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Karma = g.Sum(r => r._RatingType) })
            .OrderByDescending(x => x.Karma) // reddit moment
            .Select(x => x.Level)
            .Where(l => l != null)
            .Where(l => l._Source == (int)GameLevelSource.User)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .FilterByGameVersion(levelFilterSettings.GameVersion);

        return new DatabaseList<GameLevel>(highestRated, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetTeamPickedLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings) =>
        new(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .Where(l => l.TeamPicked)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .OrderByDescending(l => l.PublishDate), skip, count);

    [Pure]
    public DatabaseList<GameLevel> GetDeveloperLevels(int count, int skip, LevelFilterSettings levelFilterSettings) =>
        new(this._realm.All<GameLevel>()
            .Where(l => l._Source == (int)GameLevelSource.Story)
            .FilterByLevelFilterSettings(null, levelFilterSettings)
            .OrderByDescending(l => l.Title), skip, count);

    [Pure]
    public DatabaseList<GameLevel> GetBusiestLevels(int count, int skip, MatchService service, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        IOrderedEnumerable<IGrouping<GameLevel?,GameRoom>> rooms = service.Rooms
            .Where(r => r.LevelType == RoomSlotType.Online && r.HostId.Id != null) // if playing online level and host exists on server
            .GroupBy(r => this.GetLevelById(r.LevelId))
            .OrderBy(r => r.Sum(room => room.PlayerIds.Count));

        return new DatabaseList<GameLevel>(rooms.Select(r => r.Key)
            .Where(l => l != null && l._Source == (int)GameLevelSource.User)!
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .FilterByGameVersion(levelFilterSettings.GameVersion), skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetCoolLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings) =>
        new(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .Where(l => l.Score > 0)
            .OrderByDescending(l => l.Score), skip, count);

    [Pure]
    public DatabaseList<GameLevel> SearchForLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings, string query)
    {
        IQueryable<GameLevel> validLevels = this.GetLevelsByGameVersion(levelFilterSettings.GameVersion).FilterByLevelFilterSettings(user, levelFilterSettings);

        List<GameLevel> levels = validLevels.Where(l =>
                                                       QueryMethods.FullTextSearch(l.Title, query) ||
                                                       QueryMethods.FullTextSearch(l.Description, query)
        ).ToList();
        
        // If the search is just an int, then we should also look for levels which match that ID
        if (int.TryParse(query, out int id))
        {
            // Try to find a level with the ID
            GameLevel? idLevel = validLevels.FirstOrDefault(l => l.LevelId == id);

            // If we found it, and it does not duplicate, add it
            if (idLevel != null && !levels.Contains(idLevel))
            {
                levels.Add(idLevel);
            }
        }

        return new DatabaseList<GameLevel>(levels, skip, count);
    }

    [Pure]
    public int GetTotalLevelCount(TokenGame game) => this._realm.All<GameLevel>().FilterByGameVersion(game).Count(l => l._Source == (int)GameLevelSource.User);
    
    [Pure]
    public int GetTotalLevelCount() => this._realm.All<GameLevel>().Count(l => l._Source == (int)GameLevelSource.User);
    
    [Pure]
    public int GetTotalTeamPickCount(TokenGame game) => this._realm.All<GameLevel>().FilterByGameVersion(game).Count(l => l.TeamPicked);

    [Pure]
    public GameLevel? GetLevelById(int id) => this._realm.All<GameLevel>().FirstOrDefault(l => l.LevelId == id);

    private void SetLevelPickStatus(GameLevel level, bool status)
    {
        this._realm.Write(() =>
        {
            level.TeamPicked = status;
        });
    }

    public void AddTeamPickToLevel(GameLevel level) => this.SetLevelPickStatus(level, true);
    public void RemoveTeamPickFromLevel(GameLevel level) => this.SetLevelPickStatus(level, false);

    public void SetLevelScores(Dictionary<GameLevel, float> scoresToSet)
    {
        this._realm.Write(() =>
        {
            foreach ((GameLevel level, float score) in scoresToSet)
            {
                level.Score = score;
            }
        });
    }
}