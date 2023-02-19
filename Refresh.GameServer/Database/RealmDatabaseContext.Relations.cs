using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class RealmDatabaseContext // Relations
{
    public bool FavouriteLevel(GameLevel level, GameUser user)
    {
        if (this.IsLevelHeartedByUser(level, user)) return false;
        
        HeartLevelRelation relation = new(level, user);
        this._realm.Write(() =>
        {
            this._realm.Add(relation);
        });

        return true;
    }
    
    public bool UnfavouriteLevel(GameLevel level, GameUser user)
    {
        HeartLevelRelation? relation = this._realm.All<HeartLevelRelation>()
            .FirstOrDefault(r => r.Level == level && r.User == user);

        if (relation == null) return false;

        this._realm.Write(() =>
        {
            this._realm.Remove(relation);
        });

        return true;
    }

    private bool IsLevelHeartedByUser(GameLevel level, GameUser user) => this._realm.All<HeartLevelRelation>()
        .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    public IEnumerable<GameLevel> GetLevelsHeartedByUser(GameUser user, int count, int skip) => this._realm.All<HeartLevelRelation>()
        .Where(r => r.User == user)
        .AsEnumerable()
        .Select(r => r.Level)
        .Skip(skip)
        .Take(count);
}