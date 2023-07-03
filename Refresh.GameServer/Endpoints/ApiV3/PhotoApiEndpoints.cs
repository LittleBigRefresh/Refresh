using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Errors;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class PhotoApiEndpoints : EndpointGroup
{
    private static ApiListResponse<GamePhoto> PhotosByUser(RequestContext context, GameDatabaseContext database, GameUser? user)
    {
        if (user == null) return ApiNotFoundError.Instance;
        (int skip, int count) = context.GetPageData(true);

        return database.GetPhotosByUser(user, count, skip);
    }
    
    private static ApiListResponse<GamePhoto> PhotosWithUser(RequestContext context, GameDatabaseContext database, GameUser? user)
    {
        if (user == null) return ApiNotFoundError.Instance;
        (int skip, int count) = context.GetPageData(true);

        return database.GetPhotosWithUser(user, count, skip);
    }
    
    private static ApiListResponse<GamePhoto> PhotosInLevel(RequestContext context, GameDatabaseContext database, GameLevel? level)
    {
        if (level == null) return ApiNotFoundError.Instance;
        (int skip, int count) = context.GetPageData(true);

        return database.GetPhotosInLevel(level, count, skip);
    }
    
    [ApiV3Endpoint("photos/by/username/{username}"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets photos by a user by their username")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found")]
    public ApiListResponse<GamePhoto> PhotosByUsername(RequestContext context, GameDatabaseContext database,
        [DocSummary("The username of the user")] string username) 
        => PhotosByUser(context, database, database.GetUserByUsername(username));
    
    [ApiV3Endpoint("photos/with/username/{username}"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets photos with a user by their username")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found")]
    public ApiListResponse<GamePhoto> PhotosWithUsername(RequestContext context, GameDatabaseContext database,
        [DocSummary("The username of the user")] string username)
        => PhotosWithUser(context, database, database.GetUserByUsername(username));

    [ApiV3Endpoint("photos/by/uuid/{uuid}"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets photos by a user by their uuid")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found")]
    public ApiListResponse<GamePhoto> PhotosByUserUuid(RequestContext context, GameDatabaseContext database,
        [DocSummary("The UUID of the user")] string uuid)
        => PhotosByUser(context, database, database.GetUserByUuid(uuid));

    [ApiV3Endpoint("photos/with/uuid/{uuid}"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets photos with a user by their uuid")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found")]
    public ApiListResponse<GamePhoto> PhotosWithUserUuid(RequestContext context, GameDatabaseContext database,
        [DocSummary("The UUID of the user")] string uuid)
        => PhotosWithUser(context, database, database.GetUserByUuid(uuid));

    [ApiV3Endpoint("photos/in/{id}"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets photos taken in a level by its id")]
    [DocError(typeof(ApiNotFoundError), "The level cannot be found")]
    public ApiListResponse<GamePhoto> PhotosInLevelId(RequestContext context, GameDatabaseContext database,
        [DocSummary("The ID of the level")] int id)
        => PhotosInLevel(context, database, database.GetLevelById(id));
    
    [ApiV3Endpoint("photos"), Authentication(false)]
    [DocUsesPageData, DocSummary("Get all photos taken recently")]
    public ApiListResponse<GamePhoto> RecentPhotos(RequestContext context, GameDatabaseContext database)
    {
        (int skip, int count) = context.GetPageData(true);
        return database.GetRecentPhotos(count, skip);
    }
    
    [ApiV3Endpoint("photo/{id}"), Authentication(false)]
    [DocUsesPageData, DocSummary("Get an individual photo by the photo's id")]
    [DocError(typeof(ApiNotFoundError), "The photo cannot be found")]
    public GamePhoto? PhotoById(RequestContext context, GameDatabaseContext database, [DocSummary("The ID of the photo")] int id) 
        => database.GetPhotoById(id);
}