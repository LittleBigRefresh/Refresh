using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Realms;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
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

    [Pure]
    public DatabaseList<GameLevel> GetLevelsByUser(GameUser user, int count, int skip) =>
        new(this._realm.All<GameLevel>().Where(l => l.Publisher == user), skip, count);
    
    [Pure]
    public DatabaseList<GameLevel> GetNewestLevels(int count, int skip) =>
        new(this._realm.All<GameLevel>().OrderByDescending(l => l.PublishDate), skip, count);
    
    [Pure]
    public DatabaseList<GameLevel> GetRandomLevels(int count, int skip) =>
        new(this._realm.All<GameLevel>().AsEnumerable()
            .OrderBy(_ => Random.Shared.Next()), skip, count);
    
    // TODO: reduce code duplication for getting most of x
    [Pure]
    public DatabaseList<GameLevel> GetMostHeartedLevels(int count, int skip)
    {
        IQueryable<FavouriteLevelRelation> favourites = this._realm.All<FavouriteLevelRelation>();
        
        IEnumerable<GameLevel> mostHeartedLevels = favourites
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Level);

        return new DatabaseList<GameLevel>(mostHeartedLevels, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetMostUniquelyPlayedLevels(int count, int skip)
    {
        IQueryable<UniquePlayLevelRelation> uniquePlays = this._realm.All<UniquePlayLevelRelation>();
        
        IEnumerable<GameLevel> mostPlayed = uniquePlays
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Level);

        return new DatabaseList<GameLevel>(mostPlayed, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetHighestRatedLevels(int count, int skip)
    {
        IQueryable<RateLevelRelation> ratings = this._realm.All<RateLevelRelation>();
        
        IEnumerable<GameLevel> highestRated = ratings
            .AsEnumerable()
            .GroupBy(r => r.Level)
            .Select(g => new { Level = g.Key, Karma = g.Sum(r => r._RatingType) })
            .OrderByDescending(x => x.Karma) // reddit moment
            .Select(x => x.Level);

        return new DatabaseList<GameLevel>(highestRated, skip, count);
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetTeamPickedLevels(int count, int skip) =>
        new(this._realm.All<GameLevel>()
            .Where(l => l.TeamPicked)
            .OrderByDescending(l => l.PublishDate), skip, count);

    [Pure]
    public DatabaseList<GameLevel> SearchForLevels(int count, int skip, string query)
    {
        string[] keywords = query.Split(' ');
        if (keywords.Length == 0) return DatabaseList<GameLevel>.Empty();
        
        IQueryable<GameLevel> levels = this._realm.All<GameLevel>();
        
        foreach (string keyword in keywords)
        {
            if(string.IsNullOrWhiteSpace(keyword)) continue;
            
            levels = levels.Where(l =>
                // l.LevelId.ToString() == keyword ||
                QueryMethods.Like(l.Title, keyword, false) ||
                QueryMethods.Like(l.Description, keyword, false)
            );
        }

        return new DatabaseList<GameLevel>(levels, skip, count);
    }

    [Pure]
    public int GetTotalLevelCount() => this._realm.All<GameLevel>().Count();

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