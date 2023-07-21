using System.Diagnostics.Contracts;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Relations
{
    #region Favouriting Levels
    [Pure]
    private bool IsLevelFavouritedByUser(GameLevel level, GameUser user) => this._realm.All<FavouriteLevelRelation>()
        .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    [Pure]
    public DatabaseList<GameLevel> GetLevelsFavouritedByUser(GameUser user, int count, int skip) 
        => new(this._realm.All<FavouriteLevelRelation>()
        .Where(r => r.User == user)
        .AsEnumerable()
        .Select(r => r.Level), skip, count);
    
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

        this.CreateLevelFavouriteEvent(user, level);

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

    #region Favouriting Users

    [Pure]
    private bool IsUserFavouritedByUser(GameUser userToFavourite, GameUser userFavouriting) => this._realm.All<FavouriteUserRelation>()
        .FirstOrDefault(r => r.UserToFavourite == userToFavourite && r.UserFavouriting == userFavouriting) != null;

    [Pure]
    public bool AreUsersMutual(GameUser user1, GameUser user2) =>
        this.IsUserFavouritedByUser(user1, user2) &&
        this.IsUserFavouritedByUser(user2, user1);
    
    [Pure]
    public IEnumerable<GameUser> GetUsersFavouritedByUser(GameUser user, int count, int skip) => this._realm.All<FavouriteUserRelation>()
        .Where(r => r.UserFavouriting == user)
        .AsEnumerable()
        .Select(r => r.UserToFavourite)
        .Skip(skip)
        .Take(count);

    public bool FavouriteUser(GameUser userToFavourite, GameUser userFavouriting)
    {
        if (this.IsUserFavouritedByUser(userToFavourite, userFavouriting)) return false;
        
        FavouriteUserRelation relation = new()
        {
            UserToFavourite = userToFavourite,
            UserFavouriting = userFavouriting,
        };
        
        this._realm.Write(() =>
        {
            this._realm.Add(relation);
        });

        this.CreateUserFavouriteEvent(userFavouriting, userToFavourite);

        if (this.AreUsersMutual(userFavouriting, userToFavourite))
        {
            this.AddNotification("New mutual", $"You are now mutuals with {userFavouriting.Username}!", userToFavourite);
            this.AddNotification("New mutual", $"You are now mutuals with {userToFavourite.Username}!", userFavouriting);
        }
        else
        {
            this.AddNotification("New follower", $"{userFavouriting.Username} hearted you!", userToFavourite);
        }

        return true;
    }

    public bool UnfavouriteUser(GameUser userToFavourite, GameUser userFavouriting)
    {
        FavouriteUserRelation? relation = this._realm.All<FavouriteUserRelation>()
            .FirstOrDefault(r => r.UserToFavourite == userToFavourite && r.UserFavouriting == userFavouriting);

        if (relation == null) return false;

        this._realm.Write(() =>
        {
            this._realm.Remove(relation);
        });

        return true;
    }

    #endregion

    #region Queueing
    [Pure]
    private bool IsLevelQueuedByUser(GameLevel level, GameUser user) => this._realm.All<QueueLevelRelation>()
        .FirstOrDefault(r => r.Level == level && r.User == user) != null;

    [Pure]
    public DatabaseList<GameLevel> GetLevelsQueuedByUser(GameUser user, int count, int skip)
        => new(this._realm.All<QueueLevelRelation>()
        .Where(r => r.User == user)
        .AsEnumerable()
        .Select(r => r.Level), skip, count);
    
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

    public void ClearQueue(GameUser user)
    {
        this._realm.Write(() =>
        {
            this._realm.RemoveRange(user.QueueLevelRelations);
        });
    }
    #endregion

    public void PlayLevel(GameLevel level, GameUser user)
    {
        PlayLevelRelation relation = new()
        {
            Level = level,
            User = user,
            Timestamp = GetTimestampMilliseconds(),
        };
        
        UniquePlayLevelRelation? uniqueRelation = this._realm.All<UniquePlayLevelRelation>()
            .FirstOrDefault(r => r.Level == level && r.User == user);

        this._realm.Write(() =>
        {
            this._realm.Add(relation);
            
            // If the user hasn't played the level before, then add a unique relation too
            if (uniqueRelation == null) this._realm.Add(new UniquePlayLevelRelation 
            {
                Level = level,
                User = user,
                Timestamp = GetTimestampMilliseconds(),
            });
        });

        this.CreateLevelPlayEvent(user, level);
    }
}