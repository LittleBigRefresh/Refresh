using AttribDoc.Attributes;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using MongoDB.Bson;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class MatchingApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("rooms/username/{username}"), Authentication(false)]
    [DocSummary("Finds a room by a player's username")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The room could not be found")]
    public ApiResponse<ApiGameRoomResponse> GetRoomByUsername(RequestContext context, MatchService service, GameDatabaseContext database,
        [DocSummary("The username of the player")] string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return ApiNotFoundError.UserMissingError;

        GameRoom? room = service.GetRoomByPlayer(user);
        if(room == null) return ApiNotFoundError.Instance;
        
        return ApiGameRoomResponse.FromOld(room);
    }
    
    [ApiV3Endpoint("rooms/uuid/{uuid}"), Authentication(false)]
    [DocSummary("Finds a room by a player's UUID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The room could not be found")]
    public ApiResponse<ApiGameRoomResponse> GetRoomByUserUuid(RequestContext context, MatchService service, GameDatabaseContext database,
        [DocSummary("The UUID of the player")] string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;

        GameRoom? room = service.GetRoomByPlayer(user);
        if(room == null) return ApiNotFoundError.Instance;
        
        return ApiGameRoomResponse.FromOld(room);
    }
    
    [ApiV3Endpoint("rooms/{uuid}"), Authentication(false)]
    [DocSummary("Finds a room by a room's UUID")]
    [DocError(typeof(ApiValidationError), ApiValidationError.ObjectIdParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The room could not be found")]
    public ApiResponse<ApiGameRoomResponse> GetRoomByUuid(RequestContext context, MatchService service,
        [DocSummary("The UUID of the room")] string uuid)
    {
        bool parsed = ObjectId.TryParse(uuid, out ObjectId objectId);
        if (!parsed) return ApiValidationError.ObjectIdParseError;

        GameRoom? room = service.Rooms.FirstOrDefault(r => r.RoomId == objectId);
        if(room == null) return ApiNotFoundError.Instance;
        
        return ApiGameRoomResponse.FromOld(room);
    }
    
    [ApiV3Endpoint("rooms"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets all rooms on the server")]
    public ApiListResponse<ApiGameRoomResponse> GetRooms(RequestContext context, MatchService service)
    {
        (int skip, int count) = context.GetPageData(true);
        return new DatabaseList<ApiGameRoomResponse>(ApiGameRoomResponse.FromOldList(service.Rooms), skip, count);
    }
}