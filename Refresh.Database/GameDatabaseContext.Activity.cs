using System.Diagnostics;
using Bunkum.Core;
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
		
		
		IQueryable<Event> events = this.Events.FromSqlRaw(@"
	    SELECT e.* FROM public.""Events"" e
	    WHERE e.""Timestamp"" >= {0} AND e.""Timestamp"" < {1}
   		-- Exclude my levels if requested

   		
   		-- Exclude friends if requested  
   		AND ({4} = false OR 
   			((e.""StoredDataType"" != 0 OR
   			NOT EXISTS (SELECT 1 FROM public.""FavouriteUserRelations"" fur 
   				WHERE fur.""UserFavouritingId"" = {3} AND fur.""UserToFavouriteId"" = e.""StoredObjectId""))
   			AND NOT EXISTS (SELECT 1 FROM public.""FavouriteUserRelations"" fur2 
   				WHERE fur2.""UserFavouritingId"" = {3} AND fur2.""UserToFavouriteId"" = e.""UserId"")))
   							
   		-- Exclude myself if requested
   		AND ({5} = false OR 
   			(e.""UserId"" != {3} AND 
   			(e.""StoredDataType"" != 0 OR e.""StoredObjectId"" != {3}))
   	    );",
		endTimestamp, timestamp,
		parameters.ExcludeMyLevels,
		parameters.User?.UserId.ToString(),
		parameters.ExcludeFriends, 
		parameters.ExcludeMyself)
		.AsNoTracking();

		return events;
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
            List<ObjectId?> favouriteUsers = this.GetUsersFavouritedByUser(parameters.User, 1000, 0).Select(f => (ObjectId?)f.UserId).ToList();
            List<ObjectId?> userFriends = this.GetUsersMutuals(parameters.User).Select(u => (ObjectId?)u.UserId).ToList();
            List<int> userLevels = this.GetLevelsByUser(parameters.User).Select(l => l.LevelId).ToList();
            
            query = query.Where(e =>
                e.User?.UserId == parameters.User.UserId ||
                e.StoredObjectId == parameters.User.UserId ||
                favouriteUsers.Contains(e.User?.UserId) ||
                favouriteUsers.Contains(e.StoredObjectId) ||
                userFriends.Contains(e.User?.UserId) ||
                userFriends.Contains(e.StoredObjectId) ||
                (e.StoredDataType == EventDataType.Level && userLevels.Contains(e.StoredSequentialId ?? -1)) ||
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