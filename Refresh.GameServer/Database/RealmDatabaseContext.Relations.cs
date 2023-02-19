using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class RealmDatabaseContext // Relations
{
    #region Favouriting
    private bool IsLevelFavouritedByUser(GameLevel level, GameUser user) => this._realm.All<FavouriteLevelRelation>()
        .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    public IEnumerable<GameLevel> GetLevelsFavouritedByUser(GameUser user, int count, int skip) => this._realm.All<FavouriteLevelRelation>()
        .Where(r => r.User == user)
        .AsEnumerable()
        .Select(r => r.Level)
        .Skip(skip)
        .Take(count);
    
    public bool FavouriteLevel(GameLevel level, GameUser user)
    {
        if (this.IsLevelFavouritedByUser(level, user)) return false;
        
        FavouriteLevelRelation relation = new()
        {
            Level = level,
            User = user,
        };
        this._realm.Write(() =>
        {
            this._realm.Add(relation);
        });

        return true;
    }
    
    public bool UnfavouriteLevel(GameLevel level, GameUser user)
    {
        FavouriteLevelRelation? relation = this._realm.All<FavouriteLevelRelation>()
            .FirstOrDefault(r => r.Level == level && r.User == user);

        if (relation == null) return false;

        this._realm.Write(() =>
        {
            this._realm.Remove(relation);
        });

        return true;
    }
    #endregion

    #region Queueing
    private bool IsLevelQueuedByUser(GameLevel level, GameUser user) => this._realm.All<QueueLevelRelation>()
        .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    public IEnumerable<GameLevel> GetLevelsQueuedByUser(GameUser user, int count, int skip) => this._realm.All<QueueLevelRelation>()
        .Where(r => r.User == user)
        .AsEnumerable()
        .Select(r => r.Level)
        .Skip(skip)
        .Take(count);
    
    public bool QueueLevel(GameLevel level, GameUser user)
    {
        if (this.IsLevelQueuedByUser(level, user)) return false;
        
        QueueLevelRelation relation = new()
        {
            Level = level,
            User = user,
        };
        this._realm.Write(() =>
        {
            this._realm.Add(relation);
        });

        return true;
    }

    public bool DequeueLevel(GameLevel level, GameUser user)
    {
        QueueLevelRelation? relation = this._realm.All<QueueLevelRelation>()
            .FirstOrDefault(r => r.Level == level && r.User == user);

        if (relation == null) return false;

        this._realm.Write(() =>
        {
            this._realm.Remove(relation);
        });

        return true;
    }
    #endregion
}