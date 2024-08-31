using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Realms;
using Refresh.Common.Constants;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Levels
{
    public bool AddLevel(GameLevel level)
    {
        if (level.Title is { Length: > UgcLimits.TitleLimit })
            level.Title = level.Title[..UgcLimits.TitleLimit];

        if (level.Description is { Length: > UgcLimits.DescriptionLimit })
            level.Description = level.Description[..UgcLimits.DescriptionLimit];
        
        if (level.Publisher == null) throw new InvalidOperationException("Cannot create a level without a publisher");

        DateTimeOffset timestamp = this._time.Now;
        this.AddSequentialObject(level, () =>
        {
            level.PublishDate = timestamp;
            level.UpdateDate = timestamp;
        });

        return true;
    }

    public GameLevel GetStoryLevelById(int id)
    {
        GameLevel? level = this.GameLevels.FirstOrDefault(l => l.StoryId == id);

        if (level != null) return level;
        
        //Create a new level for the story level
        level = new()
        {
            Title = $"Story level #{id}",
            Publisher = null,
            Source = GameSlotType.Story,
            StoryId = id,
        };
            
        //Add the new story level to the database
        DateTimeOffset timestamp = this._time.Now;
        this.AddSequentialObject(level, () =>
        {
            level.PublishDate = timestamp;
            level.UpdateDate = timestamp;
        });
        
        return level;
    }

    public GameLevel? UpdateLevel(GameLevel newLevel, GameUser author)
    {
        if (newLevel.Title is { Length: > UgcLimits.TitleLimit })
            newLevel.Title = newLevel.Title[..UgcLimits.TitleLimit];

        if (newLevel.Description is { Length: > UgcLimits.DescriptionLimit })
            newLevel.Description = newLevel.Description[..UgcLimits.DescriptionLimit];
        
        // Verify if this level is able to be republished
        GameLevel? oldLevel = this.GetLevelById(newLevel.LevelId);
        if (oldLevel == null) return null;
            
        Debug.Assert(oldLevel.Publisher != null);
        if (oldLevel.Publisher.UserId != author.UserId) return null;
        
        // All checks passed, let's start by retaining some information from the old level
        newLevel.Publisher = author;
        newLevel.PublishDate = oldLevel.PublishDate;
        newLevel.DateTeamPicked = oldLevel.DateTeamPicked;
        
        // If the actual contents of the level haven't changed, extract some extra information
        if (oldLevel.RootResource == newLevel.RootResource)
        {
            newLevel.GameVersion = oldLevel.GameVersion;
            newLevel.UpdateDate = oldLevel.UpdateDate;
        }
        // If we're changing the actual level, update other things
        else
        {
            newLevel.UpdateDate = this._time.Now; // Set the last modified date
        }
        
        // Now newLevel is set up to replace oldLevel.
        // If information is lost here, then that's probably a bug.
        // Update the level's properties in the database
        this.Write(() =>
        {
            PropertyInfo[] userProps = typeof(GameLevel).GetProperties();
            foreach (PropertyInfo prop in userProps)
            {
                if (!prop.CanWrite || !prop.CanRead) continue;
                prop.SetValue(oldLevel, prop.GetValue(newLevel));
            }
        });

        return oldLevel;
    }
    
    public GameLevel? UpdateLevel(ApiEditLevelRequest body, GameLevel level)
    {
        if (body.Title is { Length: > UgcLimits.TitleLimit })
            body.Title = body.Title[..UgcLimits.TitleLimit];

        if (body.Description is { Length: > UgcLimits.DescriptionLimit })
            body.Description = body.Description[..UgcLimits.DescriptionLimit];
        
        this.Write(() =>
        {
            PropertyInfo[] userProps = body.GetType().GetProperties();
            foreach (PropertyInfo prop in userProps)
            {
                if (!prop.CanWrite || !prop.CanRead) continue;
                
                object? propValue = prop.GetValue(body);
                if(propValue == null) continue;

                PropertyInfo? gameLevelProp = level.GetType().GetProperty(prop.Name);
                Debug.Assert(gameLevelProp != null, $"Invalid property {prop.Name} on {nameof(ApiEditLevelRequest)}");
                
                gameLevelProp.SetValue(level, prop.GetValue(body));
            }
            
            level.UpdateDate = this._time.Now;
        });

        return level;
    }

    public void DeleteLevel(GameLevel level)
    {
        this.Write(() =>
        {
            IQueryable<Event> levelEvents = this.Events
                .Where(e => e._StoredDataType == (int)EventDataType.Level && e.StoredSequentialId == level.LevelId);
            
            this.Events.RemoveRange(levelEvents);

            this.FavouriteLevelRelations.RemoveRange(r => r.Level == level);
            this.PlayLevelRelations.RemoveRange(r => r.Level == level);
            this.QueueLevelRelations.RemoveRange(r => r.Level == level);
            this.RateLevelRelations.RemoveRange(r => r.Level == level);
            this.UniquePlayLevelRelations.RemoveRange(r => r.Level == level);
            this.TagLevelRelations.RemoveRange(r => r.Level == level);
            this.GameReviews.RemoveRange(r => r.Level == level);
            
            IQueryable<GameSubmittedScore> scores = this.GameSubmittedScores.Where(r => r.Level == level);
            
            foreach (GameSubmittedScore score in scores)
            {
                IQueryable<Event> scoreEvents = this.Events
                    .Where(e => e._StoredDataType == (int)EventDataType.Score && e.StoredObjectId == score.ScoreId);
                this.Events.RemoveRange(scoreEvents);
            }
            
            this.GameSubmittedScores.RemoveRange(scores);
        });

        //do in separate transaction in a vain attempt to fix Weirdness with favourite level relations having missing levels
        this.Write(() =>
        {
            this.GameLevels.Remove(level);
        });
    }

    private IQueryable<GameLevel> GetLevelsByGameVersion(TokenGame gameVersion) 
        => this.GameLevels.Where(l => l._Source == (int)GameSlotType.User).FilterByGameVersion(gameVersion);

    [Pure]
    public DatabaseList<GameLevel> GetLevelsByUser(GameUser user, int count, int skip, LevelFilterSettings levelFilterSettings, GameUser? accessor)
    {
        if (user.Username == SystemUsers.DeletedUserName)
        {
            return new DatabaseList<GameLevel>(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion).FilterByLevelFilterSettings(accessor, levelFilterSettings).Where(l => l.Publisher == null), skip, count);
        }

        if (user.Username == SystemUsers.UnknownUserName)
        {
            return new DatabaseList<GameLevel>(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion).FilterByLevelFilterSettings(null, levelFilterSettings).Where(l => l.IsReUpload && String.IsNullOrEmpty(l.OriginalPublisher)), skip, count);
        }
        
        if (user.Username.StartsWith(SystemUsers.SystemPrefix))
        {
            string withoutPrefix = user.Username[1..];
            return new DatabaseList<GameLevel>(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion).FilterByLevelFilterSettings(accessor, levelFilterSettings).Where(l => l.OriginalPublisher == withoutPrefix), skip, count);
        }
        
        return new DatabaseList<GameLevel>(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion).FilterByLevelFilterSettings(accessor, levelFilterSettings).Where(l => l.Publisher == user), skip, count);
    }
    
    public int GetTotalLevelsByUser(GameUser user) => this.GameLevels.Count(l => l.Publisher == user);
    
    [Pure]
    public DatabaseList<GameLevel> GetUserLevelsChunk(int skip, int count)
        => new(this.GameLevels.Where(l => l._Source == (int)GameSlotType.User), skip, count);

    [Pure]
    public IQueryable<GameLevel> GetAllUserLevels()
        => this.GameLevels.Where(l => l._Source == (int)GameSlotType.User);
    
    [Pure]
    public DatabaseList<GameLevel> GetNewestLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings) =>
        new(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .OrderByDescending(l => l.PublishDate), skip, count);
    
    [Pure]
    public DatabaseList<GameLevel> GetRandomLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        Random random = new(levelFilterSettings.Seed ?? 0);
        
        return new DatabaseList<GameLevel>(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .AsEnumerable()
            .OrderBy(_ => random.Next()), skip, count);
    }

    // TODO: reduce code duplication for getting most of x
    [Pure]
    public DatabaseList<GameLevel> GetMostHeartedLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        IQueryable<FavouriteLevelRelation> favourites = this.FavouriteLevelRelations;
        
        IEnumerable<GameLevel> mostHeartedLevels = favourites
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Level)
            .Where(l => l != null)
            .Where(l => l._Source == (int)GameSlotType.User)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .FilterByGameVersion(levelFilterSettings.GameVersion);

        return new DatabaseList<GameLevel>(mostHeartedLevels, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetLevelsByTag(int count, int skip, GameUser? user, Tag tag, LevelFilterSettings levelFilterSettings)
    {
        IQueryable<TagLevelRelation> tagRelations = this.TagLevelRelations;
        
        IEnumerable<GameLevel> filteredTaggedLevels = tagRelations
            .Where(x => x._Tag == (int)tag)
            .AsEnumerable()
            .Select(x => x.Level)
            .Distinct()
            .Where(l => l._Source == (int)GameSlotType.User)
            .OrderByDescending(l => l.PublishDate)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .FilterByGameVersion(levelFilterSettings.GameVersion);

        return new DatabaseList<GameLevel>(filteredTaggedLevels, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetMostUniquelyPlayedLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        IQueryable<UniquePlayLevelRelation> uniquePlays = this.UniquePlayLevelRelations;
        
        IEnumerable<GameLevel> mostPlayed = uniquePlays
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Level)
            .Where(l => l != null)
            .Where(l => l._Source == (int)GameSlotType.User)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .FilterByGameVersion(levelFilterSettings.GameVersion);

        return new DatabaseList<GameLevel>(mostPlayed, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetMostReplayedLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        IQueryable<PlayLevelRelation> plays = this.PlayLevelRelations;
        
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
        IQueryable<RateLevelRelation> ratings = this.RateLevelRelations;
        
        IEnumerable<GameLevel> highestRated = ratings
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Karma = g.Sum(r => r._RatingType) })
            .OrderByDescending(x => x.Karma) // reddit moment
            .Select(x => x.Level)
            .Where(l => l != null)
            .Where(l => l._Source == (int)GameSlotType.User)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .FilterByGameVersion(levelFilterSettings.GameVersion);

        return new DatabaseList<GameLevel>(highestRated, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetTeamPickedLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings) =>
        new(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .Where(l => l.DateTeamPicked != null)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .OrderByDescending(l => l.DateTeamPicked), skip, count);

    [Pure]
    public DatabaseList<GameLevel> GetDeveloperLevels(int count, int skip, LevelFilterSettings levelFilterSettings) =>
        new(this.GameLevels
            .Where(l => l._Source == (int)GameSlotType.Story)
            .FilterByLevelFilterSettings(null, levelFilterSettings)
            .OrderByDescending(l => l.Title), skip, count);

    [Pure]
    public DatabaseList<GameLevel> GetBusiestLevels(int count, int skip, MatchService service, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        IOrderedEnumerable<IGrouping<GameLevel?,GameRoom>> rooms = service.RoomAccessor.GetAllRooms()
            .Where(r => r.LevelType == RoomSlotType.Online && r.HostId.Id != null) // if playing online level and host exists on server
            .GroupBy(r => this.GetLevelById(r.LevelId))
            .OrderBy(r => r.Sum(room => room.PlayerIds.Count));

        return new DatabaseList<GameLevel>(rooms.Select(r => r.Key)
            .Where(l => l != null && l._Source == (int)GameSlotType.User)!
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
    public DatabaseList<GameLevel> GetAdventureLevels(int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings) =>
        new(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .Where(l => l.IsAdventure)
            .OrderByDescending(l => l.PublishDate), skip, count);

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
        
        // Try to look up a username to search by publisher.
        GameUser? publisher = this.GetUserByUsername(query, false); 
        if (publisher != null)
        {
            levels.AddRange(validLevels.Where(l => l.Publisher == publisher));
        }

        return new DatabaseList<GameLevel>(levels.OrderByDescending(l => l.Score), skip, count);
    }

    [Pure]
    public int GetTotalLevelCount(TokenGame game) => this.GameLevels.FilterByGameVersion(game).Count(l => l._Source == (int)GameSlotType.User);
    
    [Pure]
    public int GetTotalLevelCount() => this.GameLevels.Count(l => l._Source == (int)GameSlotType.User);

    [Pure]
    public int GetModdedLevelCount() => this.GameLevels.Count(l => l._Source == (int)GameSlotType.User && l.IsModded);

    public int GetTotalLevelsPublishedByUser(GameUser user)
        => this.GameLevels
            .Count(r => r.Publisher == user);
    
    public int GetTotalLevelsPublishedByUser(GameUser user, TokenGame game)
        => this.GameLevels
            .Count(r => r.Publisher == user && r._GameVersion == (int)game);
    
    [Pure]
    public int GetTotalTeamPickCount(TokenGame game) => this.GameLevels.FilterByGameVersion(game).Count(l => l.DateTeamPicked != null);

    [Pure]
    public GameLevel? GetLevelByIdAndType(string slotType, int id)
    {
        switch (slotType)
        {
            case "user":
                return this.GetLevelById(id);
            case "developer":
                if (id < 0)
                    return null;
                
                return this.GetStoryLevelById(id);
            default:
                return null;
        }
    }

    public GameLevel? GetLevelByRootResource(string rootResource) =>
        this.GameLevels.FirstOrDefault(level => level.RootResource == rootResource);
    
    [Pure]
    public GameLevel? GetLevelById(int id) => this.GameLevels.FirstOrDefault(l => l.LevelId == id);

    public void AddTeamPickToLevel(GameLevel level)
    {
        this.Write(() =>
        {
            level.DateTeamPicked = this._time.Now;
        });
    }

    public void RemoveTeamPickFromLevel(GameLevel level)
    {
        this.Write(() =>
        {
            level.DateTeamPicked = null;
        });
    }

    public void SetLevelScores(Dictionary<GameLevel, float> scoresToSet)
    {
        this.Write(() =>
        {
            foreach ((GameLevel level, float score) in scoresToSet)
            {
                level.Score = score;
            }
        });
    }

    public void UpdateLevelModdedStatus(GameLevel level)
    {
        this.SetLevelModdedStatus(level, this.GetLevelModdedStatus(level));
    }
    
    public bool GetLevelModdedStatus(GameLevel level)
    {
        bool modded = false;

        GameAsset? rootAsset = this.GetAssetFromHash(level.RootResource);
        
        rootAsset?.TraverseDependenciesRecursively(this, (hash, asset) =>
        {
            if (asset != null && (asset.AssetFlags & AssetFlags.Modded) != 0)
                modded = true;
        });
        
        return modded;
    }
    
    public void SetLevelModdedStatus(GameLevel level, bool modded)
    {
        this.Write(() =>
        {
            level.IsModded = modded;
        });
    }
    
    public void SetLevelModdedStatuses(Dictionary<GameLevel, bool> levels)
    {
        this.Write(() =>
        {
            foreach ((GameLevel? level, bool modded) in levels)
            {
                level.IsModded = modded;
            }
        });
    }
}