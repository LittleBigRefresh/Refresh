using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Api;

public class PhotoApiEndpoints : EndpointGroup
{
    private static IEnumerable<GamePhoto>? PhotosByUser(RequestContext context, GameDatabaseContext database, GameUser? user)
    {
        if (user == null) return null;
        (int skip, int count) = context.GetPageData(true);

        return database.GetPhotosByUser(user, count, skip);
    }
    
    private static IEnumerable<GamePhoto>? PhotosWithUser(RequestContext context, GameDatabaseContext database, GameUser? user)
    {
        if (user == null) return null;
        
        (int skip, int count) = context.GetPageData(true);

        return database.GetPhotosWithUser(user, count, skip);
    }
    
    private static IEnumerable<GamePhoto>? PhotosInLevel(RequestContext context, GameDatabaseContext database, GameLevel? level)
    {
        if (level == null) return null;
        
        (int skip, int count) = context.GetPageData(true);

        return database.GetPhotosInLevel(level, count, skip);
    }
    
    [ApiEndpoint("photos/by/username/{username}")]
    [Authentication(false)]
    public IEnumerable<GamePhoto>? PhotosByUsername(RequestContext context, GameDatabaseContext database, string username) 
        => PhotosByUser(context, database, database.GetUserByUsername(username));
    
    [ApiEndpoint("photos/with/username/{username}")]
    [Authentication(false)]
    public IEnumerable<GamePhoto>? PhotosWithUsername(RequestContext context, GameDatabaseContext database, string username)
        => PhotosWithUser(context, database, database.GetUserByUsername(username));

    [ApiEndpoint("photos/by/uuid/{uuid}")]
    [Authentication(false)]
    public IEnumerable<GamePhoto>? PhotosByUserUuid(RequestContext context, GameDatabaseContext database, string uuid)
        => PhotosByUser(context, database, database.GetUserByUuid(uuid));

    [ApiEndpoint("photos/with/uuid/{uuid}")]
    [Authentication(false)]
    public IEnumerable<GamePhoto>? PhotosWithUserUuid(RequestContext context, GameDatabaseContext database, string uuid)
        => PhotosWithUser(context, database, database.GetUserByUuid(uuid));

    [ApiEndpoint("photos/in/{id}")]
    [Authentication(false)]
    public IEnumerable<GamePhoto>? PhotosInLevelId(RequestContext context, GameDatabaseContext database, int id)
        => PhotosInLevel(context, database, database.GetLevelById(id));
}