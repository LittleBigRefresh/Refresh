using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using MongoDB.Bson;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Core.Types.Matching;
using Refresh.Database;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users.Rooms;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class MatchingApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("rooms/username/{username}"), Authentication(false)]
    [DocSummary("Finds a room by a player's username")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The room could not be found")]
    public ApiResponse<ApiGameRoomResponse> GetRoomByUsername(RequestContext context, MatchService service,
        GameDatabaseContext database,
        [DocSummary("The username of the player")]
        string username, DataContext dataContext)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return ApiNotFoundError.UserMissingError;

        GameRoom? room = service.RoomAccessor.GetRoomByUser(user);
        if(room == null) return ApiNotFoundError.Instance;
        
        return ApiGameRoomResponse.FromOld(room, dataContext);
    }
    
    [ApiV3Endpoint("rooms/uuid/{uuid}"), Authentication(false)]
    [DocSummary("Finds a room by a player's UUID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The room could not be found")]
    public ApiResponse<ApiGameRoomResponse> GetRoomByUserUuid(RequestContext context, MatchService service,
        GameDatabaseContext database,
        [DocSummary("The UUID of the player")] string uuid, DataContext dataContext)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;

        GameRoom? room = service.RoomAccessor.GetRoomByUser(user);
        if(room == null) return ApiNotFoundError.Instance;
        
        return ApiGameRoomResponse.FromOld(room, dataContext);
    }
    
    [ApiV3Endpoint("rooms/{uuid}"), Authentication(false)]
    [DocSummary("Finds a room by a room's UUID")]
    [DocError(typeof(ApiValidationError), ApiValidationError.ObjectIdParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The room could not be found")]
    public ApiResponse<ApiGameRoomResponse> GetRoomByUuid(RequestContext context, MatchService service,
        [DocSummary("The UUID of the room")] string uuid, DataContext dataContext)
    {
        bool parsed = ObjectId.TryParse(uuid, out ObjectId objectId);
        if (!parsed) return ApiValidationError.ObjectIdParseError;

        GameRoom? room = service.RoomAccessor.GetRoomByUuid(objectId);
        if(room == null) return ApiNotFoundError.Instance;
        
        return ApiGameRoomResponse.FromOld(room, dataContext);
    }
    
    [ApiV3Endpoint("rooms"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets all rooms on the server")]
    public ApiListResponse<ApiGameRoomResponse> GetRooms(RequestContext context, MatchService service,
        DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();
        return new DatabaseList<ApiGameRoomResponse>(ApiGameRoomResponse.FromOldList(service.RoomAccessor.GetAllRooms(), dataContext), skip, count);
    }
}