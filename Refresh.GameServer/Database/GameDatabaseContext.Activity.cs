using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Activity
{
    [Pure]
    public DatabaseList<Event> GetRecentActivity(
        int count, 
        int skip, 
        long timestamp, 
        long endTimestamp,
        bool excludeMyLevels = false, 
        bool excludeFriends = false,
        bool excludeFavouriteUsers = false,
        bool excludeMyself = false,
        GameUser? user = null,
        FriendStorageService? friendService = null
    )
    {
        IEnumerable<Event> query = this._realm.All<Event>()
            .Where(e => e.Timestamp < timestamp && e.Timestamp >= endTimestamp)
            .AsEnumerable();

        if (excludeMyLevels && user != null)
        {
            //Filter the query to events which either arent level related, or which the level publisher doesnt contain the user
            query = query.Where(e => this.GetLevelById(e.StoredSequentialId ?? int.MaxValue)?.Publisher?.UserId != user.UserId);
        }
        
        if (excludeFriends && user != null && friendService != null)
        {
            List<ObjectId?>? userFriends = friendService.GetUsersFriends(user, this)?.Select(u => (ObjectId?)u.UserId).ToList();

            if (userFriends != null) query = query.Where(e => !userFriends.Contains(e.StoredObjectId) &&
                                                              !userFriends.Contains(e.User.UserId));
        }

        if (excludeFavouriteUsers && user != null)
        {
            List<FavouriteUserRelation> favouriteUsers = user.UsersFavourited.ToList();
            
            query = query.Where(e => favouriteUsers.All(r => r.UserToFavourite.UserId != e.User.UserId && r.UserToFavourite.UserId != e.StoredObjectId)); 
        }

        if (excludeMyself && user != null)
        {
            query = query.Where(e => e.User.UserId != user.UserId && e.StoredObjectId != user.UserId);  
        }
        
        return new(query.OrderByDescending(e => e.Timestamp), skip, count);
    }

    [Pure]
    public DatabaseList<Event> GetRecentActivityForLevel(
        GameLevel level, 
        int count, 
        int skip, 
        long timestamp, 
        long endTimestamp,        
        bool excludeFriends = false,
        bool excludeFavouriteUsers = false,
        bool excludeMyself = false,
        GameUser? user = null,
        FriendStorageService? friendService = null
    )
    {
        IEnumerable<Event> query = this._realm.All<Event>()
            .Where(e => e._StoredDataType == 1 && e.StoredSequentialId == level.LevelId)
            .Where(e => e.Timestamp < timestamp && e.Timestamp >= endTimestamp)
            .AsEnumerable();
        
        if (excludeFriends && user != null && friendService != null)
        {
            List<ObjectId?>? userFriends = friendService.GetUsersFriends(user, this)?.Select(u => (ObjectId?)u.UserId).ToList();

            if (userFriends != null) query = query.Where(e => !userFriends.Contains(e.StoredObjectId) &&
                                                              !userFriends.Contains(e.User.UserId));
        }

        if (excludeFavouriteUsers && user != null)
        {
            List<FavouriteUserRelation> favouriteUsers = user.UsersFavourited.ToList();
            
            query = query.Where(e => favouriteUsers.All(r => r.UserToFavourite.UserId != e.User.UserId && r.UserToFavourite.UserId != e.StoredObjectId)); 
        }

        if (excludeMyself && user != null)
        {
            query = query.Where(e => e.User.UserId != user.UserId && e.StoredObjectId != user.UserId);  
        }
        
        return new DatabaseList<Event>(query
            .OrderByDescending(e => e.Timestamp), skip, count);
    }

    public int GetTotalEventCount() => this._realm.All<Event>().Count();
}