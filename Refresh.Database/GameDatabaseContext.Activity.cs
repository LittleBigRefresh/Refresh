using System.Diagnostics;
using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.Database.Query;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.Database;

public partial class GameDatabaseContext // Activity
{
    [Pure]
    public DatabaseList<Event> GetUserRecentActivity(ActivityQueryParameters parameters)
    {
        IEnumerable<Event> query = this.GetRecentActivity(parameters);

        if (parameters.User != null)
        {
            List<ObjectId?> favouriteUsers = this.GetUsersFavouritedByUser(parameters.User, 1000, 0).Select(f => (ObjectId?)f.UserId).ToList();
            List<ObjectId?> userFriends = this.GetUsersMutuals(parameters.User).Select(u => (ObjectId?)u.UserId).ToList();

            query = query.Where(e =>
                e.User.UserId == parameters.User.UserId ||
                e.StoredObjectId == parameters.User.UserId ||
                favouriteUsers.Contains(e.User.UserId) ||
                favouriteUsers.Contains(e.StoredObjectId) ||
                userFriends.Contains(e.User.UserId) ||
                userFriends.Contains(e.StoredObjectId) ||
                this.GetLevelById(e.StoredSequentialId ?? int.MaxValue)?.Publisher?.UserId == parameters.User.UserId ||
                e.EventType == EventType.LevelTeamPick ||
                e.EventType == EventType.UserFirstLogin
            );
        }
        
        return new DatabaseList<Event>(query.OrderByDescending(e => e.Timestamp), parameters.Skip, parameters.Count);
    }

    private IEnumerable<Event> GetRecentActivity(ActivityQueryParameters parameters)
    {
        if (parameters.Timestamp == 0) 
            parameters.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeMilliseconds(parameters.Timestamp);
        DateTimeOffset endTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(parameters.EndTimestamp);
        
        IEnumerable<Event> query = this.Events
            .Where(e => e.Timestamp < timestamp && e.Timestamp >= endTimestamp)
            .AsEnumerable();

        if (parameters is { ExcludeMyLevels: true, User: not null })
        {
            //Filter the query to events which either arent level related, or which the level publisher doesnt contain the user
            query = query.Where(e => e.StoredDataType != EventDataType.Level || this.GetLevelById(e.StoredSequentialId ?? int.MaxValue)?.Publisher?.UserId != parameters.User.UserId);
        }
        
        if (parameters is { ExcludeFriends: true, User: not null })
        {
            List<ObjectId?> userFriends = this.GetUsersMutuals(parameters.User).Select(u => (ObjectId?)u.UserId).ToList();

            // Filter the query to events which do not contain friends
            query = query.Where(e => (e.StoredDataType != EventDataType.User || !userFriends.Contains(e.StoredObjectId)) &&
                                                                               !userFriends.Contains(e.User.UserId));
        }

        if (parameters is { ExcludeFavouriteUsers: true, User: not null })
        {
            List<GameUser> favouriteUsers = this.GetUsersFavouritedByUser(parameters.User, 1000, 0).ToList();
            
            query = query.Where(e => favouriteUsers.All(r => r.UserId != e.User.UserId && (e.StoredDataType != EventDataType.User || r.UserId != e.StoredObjectId))); 
        }

        if (parameters is { ExcludeMyself: true, User: not null })
        {
            query = query.Where(e => e.User.UserId != parameters.User.UserId && (e.StoredDataType != EventDataType.User || e.StoredObjectId != parameters.User.UserId));  
        }

        return query;
    }

    private DatabaseActivityPage GetRecentActivityPage(ActivityQueryParameters parameters)
    {
        if (parameters.Timestamp == 0) 
            parameters.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        List<Event> events = this.GetRecentActivity(parameters).ToList();

        DatabaseActivityPage page = new(this, events);

        return page;
    }
    
    [Pure]
    public DatabaseList<Event> GetGlobalRecentActivity(ActivityQueryParameters parameters)
    {
        return new DatabaseList<Event>(this.GetRecentActivity(parameters).OrderByDescending(e => e.Timestamp), parameters.Skip, parameters.Count);
    }

    [Pure]
    public DatabaseActivityPage GetGlobalRecentActivityPage(ActivityQueryParameters parameters)
    {
        return GetRecentActivityPage(parameters);
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
    
    [Pure]
    public DatabaseList<Event> GetRecentActivityFromUser(ActivityQueryParameters parameters)
    {
        Debug.Assert(parameters.User != null);
        return new DatabaseList<Event>(this.GetRecentActivity(parameters)
            .Where(e => e.User?.UserId == parameters.User.UserId)
            .OrderByDescending(e => e.Timestamp), parameters.Skip, parameters.Count);
    }

    public int GetTotalEventCount() => this.Events.Count();
}