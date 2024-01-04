using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class RelationEndpoints : EndpointGroup
{
    [GameEndpoint("favourite/slot/user/{id}", HttpMethods.Post)]
    public Response FavouriteLevel(RequestContext context, GameDatabaseContext database, GameUser user, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474 
        if (level == null) return context.IsPSP() ? OK : NotFound;

        if (database.FavouriteLevel(level, user))
            return OK;
        
        // See above comment about PSP
        return context.IsPSP() ? OK : Unauthorized;
    }
    
    [GameEndpoint("unfavourite/slot/user/{id}", HttpMethods.Post)]
    public Response UnfavouriteLevel(RequestContext context, GameDatabaseContext database, GameUser user, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474 
        if (level == null) return context.IsPSP() ? OK : NotFound;

        if (database.UnfavouriteLevel(level, user))
            return OK;
        
        // See above comment about PSP
        return context.IsPSP() ? OK : Unauthorized;
    }
    
    [GameEndpoint("favourite/user/{username}", HttpMethods.Post)]
    public Response FavouriteUser(RequestContext context, GameDatabaseContext database, GameUser user, string username)
    {
        GameUser? userToFavourite = database.GetUserByUsername(username);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474
        if (userToFavourite == null) return context.IsPSP() ? OK : NotFound;

        if (database.FavouriteUser(userToFavourite, user))
            return OK;

        // See above comment about PSP
        return context.IsPSP() ? OK : Unauthorized;
    }
    
    [GameEndpoint("unfavourite/user/{username}", HttpMethods.Post)]
    public Response UnfavouriteUser(RequestContext context, GameDatabaseContext database, GameUser user, string username)
    {
        GameUser? userToFavourite = database.GetUserByUsername(username);
        // On PSP, we have to lie or else the client will begin spamming the server
        // https://discord.com/channels/1049223665243389953/1049225857350254632/1153468991675838474
        if (userToFavourite == null) return context.IsPSP() ? OK : NotFound;

        if (database.UnfavouriteUser(userToFavourite, user))
            return OK;

        // See above comment about PSP
        return context.IsPSP() ? OK : Unauthorized;
    }

    [GameEndpoint("favouriteUsers/{username}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedFavouriteUserList? GetFavouriteUsers(RequestContext context, GameDatabaseContext database, string username, Token token, IDataStore dataStore)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;

        (int skip, int count) = context.GetPageData();
        List<GameUser> users = database.GetUsersFavouritedByUser(user, count, skip)
            .ToList();

        return new SerializedFavouriteUserList(GameUserResponse.FromOldListWithExtraData(users, token.TokenGame, database, dataStore).ToList(), users.Count);
    }

    [GameEndpoint("lolcatftw/add/user/{id}", HttpMethods.Post)]
    public Response QueueLevel(RequestContext context, GameDatabaseContext database, GameUser user, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;
        
        if (database.QueueLevel(level, user))
            return OK;
        
        return Unauthorized;
    }
    
    [GameEndpoint("lolcatftw/remove/user/{id}", HttpMethods.Post)]
    public Response DequeueLevel(RequestContext context, GameDatabaseContext database, GameUser user, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;
        
        if (database.DequeueLevel(level, user))
            return OK;
        
        return Unauthorized;
    }

    [GameEndpoint("lolcatftw/clear", HttpMethods.Post)]
    public Response ClearQueue(RequestContext context, GameDatabaseContext database, GameUser user)
    {
        database.ClearQueue(user);
        return OK;
    }
}
