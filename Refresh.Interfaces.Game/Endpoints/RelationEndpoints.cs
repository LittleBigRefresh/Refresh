using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
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
    public Response FavouriteLevel(RequestContext context, GameDatabaseContext database, GameUser user, string slotType,
        int id, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return context.IsPSP() ? OK : Unauthorized; // See comment below
        
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474 
        if (level == null) return context.IsPSP() ? OK : NotFound;

        database.FavouriteLevel(level, user);
        return OK;
    }
    
    [GameEndpoint("unfavourite/slot/{slotType}/{id}", HttpMethods.Post)]
    [RequireEmailVerified]
    public Response UnfavouriteLevel(RequestContext context, GameDatabaseContext database, GameUser user,
        string slotType, int id, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return context.IsPSP() ? OK : Unauthorized; // See comment below

        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474 
        if (level == null) return context.IsPSP() ? OK : NotFound;

        database.UnfavouriteLevel(level, user);
        return OK;
    }
    
    [GameEndpoint("favourite/user/{username}", HttpMethods.Post)]
    [RequireEmailVerified]
    public Response FavouriteUser(RequestContext context, GameDatabaseContext database, GameUser user, string username,
        GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return context.IsPSP() ? OK : Unauthorized; // See comment below
        
        GameUser? userToFavourite = database.GetUserByUsername(username);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474
        if (userToFavourite == null) return context.IsPSP() ? OK : NotFound;

        database.FavouriteUser(userToFavourite, user);
        return OK;
    }
    
    [GameEndpoint("unfavourite/user/{username}", HttpMethods.Post)]
    [RequireEmailVerified]
    public Response UnfavouriteUser(RequestContext context, GameDatabaseContext database, GameUser user,
        string username, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return context.IsPSP() ? OK : Unauthorized; // See comment below
        
        GameUser? userToFavourite = database.GetUserByUsername(username);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474
        if (userToFavourite == null) return context.IsPSP() ? OK : NotFound;

        database.UnfavouriteUser(userToFavourite, user);
        return OK;
    }

    [GameEndpoint("favouriteUsers/{username}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
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
    public Response QueueLevel(RequestContext context, GameDatabaseContext database, GameUser user, string slotType, int id)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

        database.QueueLevel(level, user);
        return OK;
    }
    
    [GameEndpoint("lolcatftw/remove/{slotType}/{id}", HttpMethods.Post)]
    public Response DequeueLevel(RequestContext context, GameDatabaseContext database, GameUser user, string slotType, int id)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

        database.DequeueLevel(level, user);
        return OK;
    }

    [GameEndpoint("lolcatftw/clear", HttpMethods.Post)]
    public Response ClearQueue(RequestContext context, GameDatabaseContext database, GameUser user)
    {
        database.ClearQueue(user);
        return OK;
    }
    
    [GameEndpoint("tag/{slotType}/{id}", HttpMethods.Post)]
    [RequireEmailVerified]
    public Response SubmitTagsForLevel(RequestContext context, GameDatabaseContext database, GameUser user,
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
        
        return OK;
    }
}
