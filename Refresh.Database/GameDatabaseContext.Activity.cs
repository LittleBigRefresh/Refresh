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
    private IEnumerable<Event> GetEvents(ActivityQueryParameters parameters)
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

    private DatabaseActivityPage GetRecentActivity(IEnumerable<Event> eventQuery, ActivityQueryParameters parameters)
    {
        if (parameters.Timestamp == 0) 
            parameters.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        List<Event> events = eventQuery
            .OrderByDescending(e => e.Timestamp)
            .Skip(parameters.Skip)
            .Take(parameters.Count)
            .ToList();

        DatabaseActivityPage page = new(this, events);

        return page;
    }
    
    [Pure]
    public DatabaseList<Event> GetGlobalRecentActivity(ActivityQueryParameters parameters)
    {
        return new DatabaseList<Event>(this.GetEvents(parameters).OrderByDescending(e => e.Timestamp), parameters.Skip, parameters.Count);
    }

    [Pure]
    public DatabaseActivityPage GetGlobalRecentActivityPage(ActivityQueryParameters parameters)
    {
        return this.GetRecentActivity(this.GetEvents(parameters), parameters);
    }
    
    [Pure]
    public DatabaseActivityPage GetUserRecentActivity(ActivityQueryParameters parameters)
    {
        IEnumerable<Event> query = this.GetEvents(parameters);

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

        query = query.OrderByDescending(e => e.Timestamp);

        return GetRecentActivity(query, parameters);
    }

    [Pure]
    public DatabaseActivityPage GetRecentActivityForLevel(
        GameLevel level, 
        ActivityQueryParameters parameters
    )
    {
        IEnumerable<Event> events = this.GetEvents(parameters)
            .Where(e => e._StoredDataType == (int)EventDataType.Level && e.StoredSequentialId == level.LevelId)
            .OrderByDescending(e => e.Timestamp);

        return GetRecentActivity(events, parameters);
    }
    
    [Pure]
    public DatabaseActivityPage GetRecentActivityFromUser(ActivityQueryParameters parameters)
    {
        Debug.Assert(parameters.User != null);

        IEnumerable<Event> events = this.GetEvents(parameters)
            .Where(e => e.User?.UserId == parameters.User.UserId)
            .OrderByDescending(e => e.Timestamp);
        
        return GetRecentActivity(events, parameters);
    }

    public int GetTotalEventCount() => this.Events.Count();
}