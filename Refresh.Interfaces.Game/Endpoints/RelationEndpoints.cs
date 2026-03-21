using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.RateLimits.Relations;
using Refresh.Core.RateLimits.Users;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Endpoints;

public class RelationEndpoints : EndpointGroup
{
    [GameEndpoint("favourite/slot/{slotType}/{id}", HttpMethods.Post)]
    [RequireEmailVerified]
    [RateLimitSettings(CommonRelationEndpointLimits.TimeoutDuration, CommonRelationEndpointLimits.RequestAmount, 
                            CommonRelationEndpointLimits.BlockDuration, CommonRelationEndpointLimits.RequestBucket)]
    public Response FavouriteLevel(RequestContext context, GameDatabaseContext database, GameUser user, string slotType,
        int id, GameServerConfig config, DataContext dataContext)
    {
        if (user.IsWriteBlocked(config))
            return context.IsPSP() ? OK : Unauthorized; // See comment below
        
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474 
        if (level == null) return context.IsPSP() ? OK : NotFound;

        database.FavouriteLevel(level, user);
        dataContext.Cache.UpdateLevelHeartedStatusByUser(user, level, true, database);
        return OK;
    }
    
    [GameEndpoint("unfavourite/slot/{slotType}/{id}", HttpMethods.Post)]
    [RequireEmailVerified]
    [RateLimitSettings(CommonRelationEndpointLimits.TimeoutDuration, CommonRelationEndpointLimits.RequestAmount, 
                            CommonRelationEndpointLimits.BlockDuration, CommonRelationEndpointLimits.RequestBucket)]
    public Response UnfavouriteLevel(RequestContext context, GameDatabaseContext database, GameUser user,
        string slotType, int id, GameServerConfig config, DataContext dataContext)
    {
        if (user.IsWriteBlocked(config))
            return context.IsPSP() ? OK : Unauthorized; // See comment below

        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474 
        if (level == null) return context.IsPSP() ? OK : NotFound;

        database.UnfavouriteLevel(level, user);
        dataContext.Cache.UpdateLevelHeartedStatusByUser(user, level, false, database);
        return OK;
    }
    
    [GameEndpoint("favourite/user/{username}", HttpMethods.Post)]
    [RequireEmailVerified]
    [RateLimitSettings(CommonRelationEndpointLimits.TimeoutDuration, CommonRelationEndpointLimits.RequestAmount, 
                            CommonRelationEndpointLimits.BlockDuration, CommonRelationEndpointLimits.RequestBucket)]
    public Response FavouriteUser(RequestContext context, GameDatabaseContext database, GameUser user, string username,
        GameServerConfig config, DataContext dataContext)
    {
        if (user.IsWriteBlocked(config))
            return context.IsPSP() ? OK : Unauthorized; // See comment below
        
        GameUser? userToFavourite = database.GetUserByUsername(username);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474
        if (userToFavourite == null) return context.IsPSP() ? OK : NotFound;

        database.FavouriteUser(userToFavourite, user);
        dataContext.Cache.UpdateUserHeartedStatusByUser(user, userToFavourite, true, database);
        return OK;
    }
    
    [GameEndpoint("unfavourite/user/{username}", HttpMethods.Post)]
    [RequireEmailVerified]
    [RateLimitSettings(CommonRelationEndpointLimits.TimeoutDuration, CommonRelationEndpointLimits.RequestAmount, 
                            CommonRelationEndpointLimits.BlockDuration, CommonRelationEndpointLimits.RequestBucket)]
    public Response UnfavouriteUser(RequestContext context, GameDatabaseContext database, GameUser user,
        string username, GameServerConfig config, DataContext dataContext)
    {
        if (user.IsWriteBlocked(config))
            return context.IsPSP() ? OK : Unauthorized; // See comment below
        
        GameUser? userToFavourite = database.GetUserByUsername(username);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474
        if (userToFavourite == null) return context.IsPSP() ? OK : NotFound;

        database.UnfavouriteUser(userToFavourite, user);
        dataContext.Cache.UpdateUserHeartedStatusByUser(user, userToFavourite, false, database);
        return OK;
    }

    [GameEndpoint("favouriteUsers/{username}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    [RateLimitSettings(UserListEndpointLimits.TimeoutDuration, UserListEndpointLimits.RequestAmount, 
                            UserListEndpointLimits.BlockDuration, UserListEndpointLimits.RequestBucket)]
    public SerializedFavouriteUserList? GetFavouriteUsers(RequestContext context, GameDatabaseContext database,
        string username, DataContext dataContext)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;

        (int skip, int count) = context.GetPageData();

        DatabaseList<GameUser> users = database.GetUsersFavouritedByUser(user, skip, count);
        return new SerializedFavouriteUserList(GameUserResponse.FromOldList(users.Items.ToArray(), dataContext).ToList(), users.TotalItems, users.NextPageIndex);
    }

    [GameEndpoint("lolcatftw/add/{slotType}/{id}", HttpMethods.Post)]
    [RequireEmailVerified]
    [RateLimitSettings(CommonRelationEndpointLimits.TimeoutDuration, CommonRelationEndpointLimits.RequestAmount, 
                            CommonRelationEndpointLimits.BlockDuration, CommonRelationEndpointLimits.RequestBucket)]
    public Response QueueLevel(RequestContext context, GameDatabaseContext database, GameUser user, string slotType, int id, DataContext dataContext, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config)) 
            return Unauthorized;

        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

        database.QueueLevel(level, user);
        dataContext.Cache.UpdateLevelQueuedStatusByUser(user, level, true, database);
        return OK;
    }
    
    [GameEndpoint("lolcatftw/remove/{slotType}/{id}", HttpMethods.Post)]
    [RequireEmailVerified]
    [RateLimitSettings(CommonRelationEndpointLimits.TimeoutDuration, CommonRelationEndpointLimits.RequestAmount, 
                            CommonRelationEndpointLimits.BlockDuration, CommonRelationEndpointLimits.RequestBucket)]
    public Response DequeueLevel(RequestContext context, GameDatabaseContext database, GameUser user, string slotType, int id, DataContext dataContext, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config)) 
            return Unauthorized;

        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

        database.DequeueLevel(level, user);
        dataContext.Cache.UpdateLevelQueuedStatusByUser(user, level, false, database);
        return OK;
    }

    [GameEndpoint("lolcatftw/clear", HttpMethods.Post)]
    [RequireEmailVerified]
    [RateLimitSettings(CommonRelationEndpointLimits.TimeoutDuration, CommonRelationEndpointLimits.RequestAmount, 
                            CommonRelationEndpointLimits.BlockDuration, CommonRelationEndpointLimits.RequestBucket)]
    public Response ClearQueue(RequestContext context, GameDatabaseContext database, GameUser user, DataContext dataContext, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config)) 
            return Unauthorized;

        database.ClearQueue(user);
        dataContext.Cache.ClearQueueByUser(user);
        return OK;
    }
    
    [GameEndpoint("tag/{slotType}/{id}", HttpMethods.Post)]
    [RequireEmailVerified]
    [RateLimitSettings(LevelTaggingEndpointLimits.TimeoutDuration, LevelTaggingEndpointLimits.RequestAmount, 
                            LevelTaggingEndpointLimits.BlockDuration, LevelTaggingEndpointLimits.RequestBucket)]
    public Response SubmitTagsForLevel(RequestContext context, GameDatabaseContext database, GameUser user, DataContext dataContext,
        string slotType, int id, string body, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        
        if (level == null)
            return NotFound;

        // The format of the POST body is `t=TAG_Name`, so assert this is followed
        if (!body.StartsWith("t="))
            return BadRequest;

        Tag? tag = TagExtensions.FromLbpString(body[2..]);

        // If it was an invalid tag, return BadRequest
        if (tag == null)
            return BadRequest;
        
        database.AddTagRelation(user, level, tag.Value);
        dataContext.Cache.RemoveTags(level); // Reset
        
        return OK;
    }
}
