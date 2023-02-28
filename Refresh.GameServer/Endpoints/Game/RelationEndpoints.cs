using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class RelationEndpoints : EndpointGroup
{
    [GameEndpoint("favourite/slot/user/{idStr}", Method.Post)]
    public Response FavouriteLevel(RequestContext context, RealmDatabaseContext database, GameUser user, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return new Response(HttpStatusCode.BadRequest);
        
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return new Response(HttpStatusCode.NotFound);

        if (database.FavouriteLevel(level, user))
            return new Response(HttpStatusCode.OK);
        
        return new Response(HttpStatusCode.Unauthorized);
    }
    
    [GameEndpoint("unfavourite/slot/user/{idStr}", Method.Post)]
    public Response UnfavouriteLevel(RequestContext context, RealmDatabaseContext database, GameUser user, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return new Response(HttpStatusCode.BadRequest);
        
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return new Response(HttpStatusCode.NotFound);

        if (database.UnfavouriteLevel(level, user))
            return new Response(HttpStatusCode.OK);
        
        return new Response(HttpStatusCode.Unauthorized);
    }
    
    [GameEndpoint("favourite/user/{username}", Method.Post)]
    public Response FavouriteUser(RequestContext context, RealmDatabaseContext database, GameUser user, string username)
    {
        GameUser? userToFavourite = database.GetUserByUsername(username);
        if (userToFavourite == null) return new Response(HttpStatusCode.NotFound);

        if (database.FavouriteUser(userToFavourite, user))
            return new Response(HttpStatusCode.OK);
        
        return new Response(HttpStatusCode.Unauthorized);
    }
    
    [GameEndpoint("unfavourite/user/{username}", Method.Post)]
    public Response UnfavouriteUser(RequestContext context, RealmDatabaseContext database, GameUser user, string username)
    {
        GameUser? userToFavourite = database.GetUserByUsername(username);
        if (userToFavourite == null) return new Response(HttpStatusCode.NotFound);

        if (database.UnfavouriteUser(userToFavourite, user))
            return new Response(HttpStatusCode.OK);
        
        return new Response(HttpStatusCode.Unauthorized);
    }

    [GameEndpoint("favouriteUsers/{username}", ContentType.Xml)]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public GameFavouriteUserList? GetFavouriteUsers(RequestContext context, RealmDatabaseContext database, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;

        (int skip, int count) = context.GetPageData();
        List<GameUser> users = database.GetUsersFavouritedByUser(user, count, skip)
            .ToList();
        
        foreach (GameUser favouritedUser in users) favouritedUser.PrepareForSerialization();

        return new GameFavouriteUserList(users, users.Count);
    }

    [GameEndpoint("lolcatftw/add/user/{idStr}", Method.Post)]
    public Response QueueLevel(RequestContext context, RealmDatabaseContext database, GameUser user, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return new Response(HttpStatusCode.BadRequest);
        
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return new Response(HttpStatusCode.NotFound);
        
        if (database.QueueLevel(level, user))
            return new Response(HttpStatusCode.OK);
        
        return new Response(HttpStatusCode.Unauthorized);
    }
    
    [GameEndpoint("lolcatftw/remove/user/{idStr}", Method.Post)]
    public Response DequeueLevel(RequestContext context, RealmDatabaseContext database, GameUser user, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return new Response(HttpStatusCode.BadRequest);
        
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return new Response(HttpStatusCode.NotFound);
        
        if (database.DequeueLevel(level, user))
            return new Response(HttpStatusCode.OK);
        
        return new Response(HttpStatusCode.Unauthorized);
    }
}