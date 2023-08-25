using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;

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

    public void DeleteLevel(GameLevel level)
    {
        this._realm.Write(() =>
        {
            IQueryable<Event> events = this._realm.All<Event>()
                .Where(e => e._StoredDataType == (int)EventDataType.Level && e.StoredSequentialId == level.LevelId);
            
            this._realm.RemoveRange(events);
            this._realm.Remove(level);
        });
    }
    

    private IQueryable<GameLevel> GetLevelsByGameVersion(TokenGame gameVersion) 
        => this._realm.All<GameLevel>().FilterByGameVersion(gameVersion);

    [Pure]
    public DatabaseList<GameLevel> GetLevelsByUser(GameUser user, int count, int skip, TokenGame gameVersion) =>
        new(this.GetLevelsByGameVersion(gameVersion).Where(l => l.Publisher == user), skip, count);
    
    [Pure]
    public DatabaseList<GameLevel> GetNewestLevels(int count, int skip, TokenGame gameVersion) =>
        new(this.GetLevelsByGameVersion(gameVersion).OrderByDescending(l => l.PublishDate), skip, count);
    
    [Pure]
    public DatabaseList<GameLevel> GetRandomLevels(int count, int skip, TokenGame gameVersion) =>
        new(this.GetLevelsByGameVersion(gameVersion).AsEnumerable()
            .OrderBy(_ => Random.Shared.Next()), skip, count);
    
    // TODO: reduce code duplication for getting most of x
    [Pure]
    public DatabaseList<GameLevel> GetMostHeartedLevels(int count, int skip, TokenGame gameVersion)
    {
        IQueryable<FavouriteLevelRelation> favourites = this._realm.All<FavouriteLevelRelation>();
        
        IEnumerable<GameLevel> mostHeartedLevels = favourites
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Level)
            .FilterByGameVersion(gameVersion);

        return new DatabaseList<GameLevel>(mostHeartedLevels, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetMostUniquelyPlayedLevels(int count, int skip, TokenGame gameVersion)
    {
        IQueryable<UniquePlayLevelRelation> uniquePlays = this._realm.All<UniquePlayLevelRelation>();
        
        IEnumerable<GameLevel> mostPlayed = uniquePlays
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Level)
            .FilterByGameVersion(gameVersion);

        return new DatabaseList<GameLevel>(mostPlayed, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetHighestRatedLevels(int count, int skip, TokenGame gameVersion)
    {
        IQueryable<RateLevelRelation> ratings = this._realm.All<RateLevelRelation>();
        
        IEnumerable<GameLevel> highestRated = ratings
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Karma = g.Sum(r => r._RatingType) })
            .OrderByDescending(x => x.Karma) // reddit moment
            .Select(x => x.Level)
            .FilterByGameVersion(gameVersion);

        return new DatabaseList<GameLevel>(highestRated, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetTeamPickedLevels(int count, int skip, TokenGame gameVersion) =>
        new(this.GetLevelsByGameVersion(gameVersion)
            .Where(l => l.TeamPicked)
            .OrderByDescending(l => l.PublishDate), skip, count);

    [Pure]
    public DatabaseList<GameLevel> GetBusiestLevels(int count, int skip, TokenGame gameVersion, MatchService service)
    {
        IOrderedEnumerable<IGrouping<GameLevel?,GameRoom>> rooms = service.Rooms
            .Where(r => r.LevelType == RoomSlotType.Online && r.HostId.Id != null) // if playing online level and host exists on server
            .GroupBy(r => this.GetLevelById(r.LevelId))
            .OrderBy(r => r.Sum(room => room.PlayerIds.Count));

        return new DatabaseList<GameLevel>(rooms.Select(r => r.Key)
            .Where(l => l != null)!.FilterByGameVersion(gameVersion), skip, count);
    }

    [Pure]
    public DatabaseList<GameLevel> SearchForLevels(int count, int skip, TokenGame gameVersion, string query)
    {
        IQueryable<GameLevel> validLevels = this.GetLevelsByGameVersion(gameVersion);

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
    public int GetTotalLevelCount() => this._realm.All<GameLevel>().Count();
    
    [Pure]
    public int GetTotalTeamPickCount() => this._realm.All<GameLevel>().Count(l => l.TeamPicked);

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
}