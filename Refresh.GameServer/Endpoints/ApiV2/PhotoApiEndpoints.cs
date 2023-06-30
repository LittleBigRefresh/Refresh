using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV2;

public class PhotoApiEndpoints : EndpointGroup
{
    private static IEnumerable<GamePhoto>? PhotosByUser(RequestContext context, GameDatabaseContext database, GameUser? user)
    {
        if (user == null) return null;
        (int skip, int count) = context.GetPageData(true);

        return database.GetPhotosByUser(user, count, skip).Items;
    }
    
    private static IEnumerable<GamePhoto>? PhotosWithUser(RequestContext context, GameDatabaseContext database, GameUser? user)
    {
        if (user == null) return null;
        
        (int skip, int count) = context.GetPageData(true);

        return database.GetPhotosWithUser(user, count, skip).Items;
    }
    
    private static IEnumerable<GamePhoto>? PhotosInLevel(RequestContext context, GameDatabaseContext database, GameLevel? level)
    {
        if (level == null) return null;
        
        (int skip, int count) = context.GetPageData(true);

        return database.GetPhotosInLevel(level, count, skip).Items;
    }
    
    [ApiV2Endpoint("photos/by/username/{username}")]
    [Authentication(false)]
    public IEnumerable<GamePhoto>? PhotosByUsername(RequestContext context, GameDatabaseContext database, string username) 
        => PhotosByUser(context, database, database.GetUserByUsername(username));
    
    [ApiV2Endpoint("photos/with/username/{username}")]
    [Authentication(false)]
    public IEnumerable<GamePhoto>? PhotosWithUsername(RequestContext context, GameDatabaseContext database, string username)
        => PhotosWithUser(context, database, database.GetUserByUsername(username));

    [ApiV2Endpoint("photos/by/uuid/{uuid}")]
    [Authentication(false)]
    public IEnumerable<GamePhoto>? PhotosByUserUuid(RequestContext context, GameDatabaseContext database, string uuid)
        => PhotosByUser(context, database, database.GetUserByUuid(uuid));

    [ApiV2Endpoint("photos/with/uuid/{uuid}")]
    [Authentication(false)]
    public IEnumerable<GamePhoto>? PhotosWithUserUuid(RequestContext context, GameDatabaseContext database, string uuid)
        => PhotosWithUser(context, database, database.GetUserByUuid(uuid));

    [ApiV2Endpoint("photos/in/{id}")]
    [Authentication(false)]
    public IEnumerable<GamePhoto>? PhotosInLevelId(RequestContext context, GameDatabaseContext database, int id)
        => PhotosInLevel(context, database, database.GetLevelById(id));
    
    [ApiV2Endpoint("photos")]
    [Authentication(false)]
    public IEnumerable<GamePhoto> RecentPhotos(RequestContext context, GameDatabaseContext database)
    {
        (int skip, int count) = context.GetPageData(true);
        return database.GetRecentPhotos(count, skip).Items;
    }
    
    [ApiV2Endpoint("photo/{id}")]
    [Authentication(false)]
    [NullStatusCode(NotFound)]
    public GamePhoto? PhotoById(RequestContext context, GameDatabaseContext database, int id) 
        => database.GetPhotoById(id);
}