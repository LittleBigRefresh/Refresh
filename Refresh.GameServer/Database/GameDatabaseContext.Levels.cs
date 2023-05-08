using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Realms;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Levels
{
    public bool AddLevel(GameLevel level)
    {
        if (level.Publisher == null) throw new ArgumentNullException(nameof(level.Publisher));

        long timestamp = GetTimestampMilliseconds();
        this.AddSequentialObject(level, () =>
        {
            level.PublishDate = timestamp;
            level.UpdateDate = timestamp;
        });

        return true;
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
            oldSlot.UpdateDate = GetTimestampMilliseconds();
        });

        return oldSlot;
    }

    public void DeleteLevel(GameLevel level)
    {
        this._realm.Write(() =>
        {
            IQueryable<Event> events = this._realm.All<Event>()
                .Where(e => e.StoredDataType == EventDataType.Level && e.StoredSequentialId == level.LevelId);
            
            this._realm.RemoveRange(events);
            this._realm.Remove(level);
        });
    }

    [Pure]
    public IEnumerable<GameLevel> GetLevelsByUser(GameUser user, int count, int skip) =>
        this._realm.All<GameLevel>()
            .Where(l => l.Publisher == user)
            .AsEnumerable()
            .Skip(skip)
            .Take(count);
    
    [Pure]
    public IEnumerable<GameLevel> GetNewestLevels(int count, int skip) =>
        this._realm.All<GameLevel>()
            .OrderByDescending(l => l.PublishDate)
            .AsEnumerable()
            .Skip(skip)
            .Take(count);
    
    [Pure]
    public IEnumerable<GameLevel> GetRandomLevels(int count, int skip)
    {
        return this._realm.All<GameLevel>()
            .AsEnumerable()
            .OrderBy(_ => Random.Shared.Next())
            .Skip(skip)
            .Take(count);
    }

    // FIXME: to get this to work with new categories I removed the total number of results, this is terrible
    [Pure]
    public IEnumerable<GameLevel> SearchForLevels(int count, int skip, string query)
    {
        string[] keywords = query.Split(' ');
        if (keywords.Length == 0) return Array.Empty<GameLevel>();
        
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

        return levels.AsEnumerable().Skip(skip).Take(count);
    }

    [Pure]
    public int GetTotalLevelCount() => this._realm.All<GameLevel>().Count();

    [Pure]
    public GameLevel? GetLevelById(int id) => this._realm.All<GameLevel>().FirstOrDefault(l => l.LevelId == id);
}