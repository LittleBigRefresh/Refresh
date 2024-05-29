using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Matching;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users.Rooms;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameRoomResponse : IApiResponse, IDataConvertableFrom<ApiGameRoomResponse, GameRoom>
{
    public required string RoomId { get; set; }
    public required IEnumerable<ApiGameRoomPlayerResponse> PlayerIds { get; set; }
    public required RoomState RoomState { get; set; }
    public required RoomMood RoomMood { get; set; }
    public required RoomSlotType LevelType { get; set; }
    public required int LevelId { get; set; }
    
    public required TokenPlatform Platform { get; set; }
    public required TokenGame Game { get; set; }
    
    public static ApiGameRoomResponse? FromOld(GameRoom? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiGameRoomResponse
        {
            RoomId = old.RoomId.ToString()!,
            PlayerIds = ApiGameRoomPlayerResponse.FromOldList(old.PlayerIds, dataContext),
            RoomState = old.RoomState,
            RoomMood = old.RoomMood,
            LevelType = old.LevelType,
            LevelId = old.LevelId,
            Platform = old.Platform,
            Game = old.Game,
        };
    }

    public static IEnumerable<ApiGameRoomResponse> FromOldList(IEnumerable<GameRoom> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}