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

        if (parameters.EndTimestamp == 0)
            parameters.EndTimestamp = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(30.5 * 3)).ToUnixTimeMilliseconds();

        DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeMilliseconds(parameters.Timestamp);
        DateTimeOffset endTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(parameters.EndTimestamp);

        // DO NOT USE INCLUDE ON THIS QUERY
        // this will severely hurt performance as postgres will not use the index efficiently due to the joins
        IEnumerable<Event> query = this.Events
            .Where(e => e.Timestamp < timestamp && e.Timestamp >= endTimestamp);

        if (parameters is { ExcludeMyLevels: true, User: not null })
        {
            //Filter the query to events which either arent level related, or which the level publisher is not the user
            query = query // TODO: This part of the query can still be more optimized
                .ToArray() // this instead of AsEnumerable avoids a NpgsqlOperationInProgressException
                .Where(e => e.StoredDataType != EventDataType.Level || (e.StoredSequentialId != null && this.GetLevelById(e.StoredSequentialId.Value)?.Publisher?.UserId != parameters.User.UserId));
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
            List<GameUser> favouriteUsers = this.GetUsersFavouritedByUser(parameters.User, 0, 1000).Items.ToList();
            
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

        // step 1: gather event ids
        List<ObjectId> eventIds = eventQuery
            .OrderByDescending(e => e.Timestamp)
            .Select(e => e.EventId)
            .Skip(parameters.Skip)
            .Take(parameters.Count)
            .ToList();
        
        Debug.Assert(eventIds.Count <= 100);

        // step 2: separate query to contain JOINs
        // helps with performance
        List<Event> events = this.Events
            .Where(e => eventIds.Contains(e.EventId))
            .Include(e => e.User)
            .Include(e => e.User.Statistics)
            .AsEnumerable()
            .OrderByDescending(e => e.Timestamp)
            .ToList();

        DatabaseActivityPage page = new(this, events, parameters);

        return page;
    }
    
    [Pure]
    public DatabaseList<Event> GetGlobalRecentActivity(ActivityQueryParameters parameters)
    {
        return new DatabaseList<Event>(this.GetEvents(parameters), parameters.Skip, parameters.Count);
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
            List<ObjectId?> favouriteUsers = this.GetUsersFavouritedByUser(parameters.User, 0, 1000).Items.Select(f => (ObjectId?)f.UserId).ToList();
            List<ObjectId?> userFriends = this.GetUsersMutuals(parameters.User).Select(u => (ObjectId?)u.UserId).ToList();

            query = query.Where(e =>
                e.User?.UserId == parameters.User.UserId ||
                e.StoredObjectId == parameters.User.UserId ||
                favouriteUsers.Contains(e.User?.UserId) ||
                favouriteUsers.Contains(e.StoredObjectId) ||
                userFriends.Contains(e.User?.UserId) ||
                userFriends.Contains(e.StoredObjectId) ||
                #if false // FIXME: double query
                this.GetLevelById(e.StoredSequentialId ?? int.MaxValue)?.Publisher?.UserId == parameters.User.UserId ||
                #endif
                e.EventType == EventType.LevelTeamPick ||
                e.EventType == EventType.UserFirstLogin
            );
        }

        return GetRecentActivity(query, parameters);
    }

    [Pure]
    public DatabaseActivityPage GetRecentActivityForLevel(
        GameLevel level, 
        ActivityQueryParameters parameters
    )
    {
        IEnumerable<Event> events = this.GetEvents(parameters)
            .Where(e => e.StoredDataType == EventDataType.Level && e.StoredSequentialId == level.LevelId);

        return GetRecentActivity(events, parameters);
    }
    
    [Pure]
    public DatabaseActivityPage GetRecentActivityFromUser(ActivityQueryParameters parameters)
    {
        Debug.Assert(parameters.User != null);

        IEnumerable<Event> events = this.GetEvents(parameters)
            .Where(e => e.User?.UserId == parameters.User.UserId);
        
        return GetRecentActivity(events, parameters);
    }

    public int GetTotalEventCount() => this.Events.Count();
}