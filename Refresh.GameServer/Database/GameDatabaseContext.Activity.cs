using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial interface IGameDatabaseContext // Activity
{
    [Pure]
    public DatabaseList<Event> GetUserRecentActivity(ActivityQueryParameters parameters)
    {
        IEnumerable<Event> query = this.GetRecentActivity(parameters);

        if (parameters.User != null)
        {
            List<ObjectId?> favouriteUsers = parameters.User.UsersFavourited.AsEnumerable().Select(f => (ObjectId?)f.UserToFavourite.UserId).ToList();
            List<ObjectId?> userFriends = this.GetUsersMutuals(parameters.User).Select(u => (ObjectId?)u.UserId).ToList();

            query = query.Where(e =>
                e.User.UserId == parameters.User.UserId ||
                e.StoredObjectId == parameters.User.UserId ||
                favouriteUsers.Contains(e.User.UserId) ||
                favouriteUsers.Contains(e.StoredObjectId) ||
                userFriends.Contains(e.User.UserId) ||
                userFriends.Contains(e.StoredObjectId) ||
                this.GetLevelById(e.StoredSequentialId ?? int.MaxValue)?.Publisher?.UserId == parameters.User.UserId ||
                e.EventType == EventType.Level_TeamPick ||
                e.EventType == EventType.User_FirstLogin
            );
        }
        
        return new DatabaseList<Event>(query.OrderByDescending(e => e.Timestamp), parameters.Skip, parameters.Count);
    }

    private IEnumerable<Event> GetRecentActivity(ActivityQueryParameters parameters)
    {
        if (parameters.Timestamp == 0) 
            parameters.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        IEnumerable<Event> query = this.All<Event>()
            .Where(e => e.Timestamp < parameters.Timestamp && e.Timestamp >= parameters.EndTimestamp)
            .AsEnumerable();

        if (parameters is { ExcludeMyLevels: true, User: not null })
        {
            //Filter the query to events which either arent level related, or which the level publisher doesnt contain the user
            query = query.Where(e => this.GetLevelById(e.StoredSequentialId ?? int.MaxValue)?.Publisher?.UserId != parameters.User.UserId);
        }
        
        if (parameters is { ExcludeFriends: true, User: not null })
        {
            List<ObjectId?> userFriends = this.GetUsersMutuals(parameters.User).Select(u => (ObjectId?)u.UserId).ToList();

            query = query.Where(e => !userFriends.Contains(e.StoredObjectId) &&
                                     !userFriends.Contains(e.User.UserId));
        }

        if (parameters is { ExcludeFavouriteUsers: true, User: not null })
        {
            List<FavouriteUserRelation> favouriteUsers = parameters.User.UsersFavourited.ToList();
            
            query = query.Where(e => favouriteUsers.All(r => r.UserToFavourite.UserId != e.User.UserId && r.UserToFavourite.UserId != e.StoredObjectId)); 
        }

        if (parameters is { ExcludeMyself: true, User: not null })
        {
            query = query.Where(e => e.User.UserId != parameters.User.UserId && e.StoredObjectId != parameters.User.UserId);  
        }

        return query;
    }
    
    [Pure]
    public DatabaseList<Event> GetGlobalRecentActivity(ActivityQueryParameters parameters)
    {
        return new DatabaseList<Event>(this.GetRecentActivity(parameters).OrderByDescending(e => e.Timestamp), parameters.Skip, parameters.Count);
    }

    [Pure]
    public DatabaseList<Event> GetRecentActivityForLevel(
        GameLevel level, 
        ActivityQueryParameters parameters
    )
    {
        return new DatabaseList<Event>(this.GetRecentActivity(parameters)
            .Where(e => e._StoredDataType == 1 && e.StoredSequentialId == level.LevelId)
            .OrderByDescending(e => e.Timestamp), parameters.Skip, parameters.Count);
    }

    public int GetTotalEventCount() => this.All<Event>().Count();
}