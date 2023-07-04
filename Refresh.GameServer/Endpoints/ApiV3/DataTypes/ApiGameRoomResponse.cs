using Refresh.GameServer.Types.Matching;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameRoomResponse : IApiResponse, IDataConvertableFrom<ApiGameRoomResponse, GameRoom>
{
    public required string RoomId { get; set; }
    public required List<GameRoomPlayer> PlayerIds { get; set; }
    public required RoomState RoomState { get; set; }
    public required RoomMood RoomMood { get; set; }
    public required RoomSlotType LevelType { get; set; }
    public required int LevelId { get; set; }
    
    public static ApiGameRoomResponse? FromOld(GameRoom? old)
    {
        if (old == null) return null;

        return new ApiGameRoomResponse
        {
            RoomId = old.RoomId.ToString()!,
            PlayerIds = old.PlayerIds,
            RoomState = old.RoomState,
            RoomMood = old.RoomMood,
            LevelType = old.LevelType,
            LevelId = old.LevelId,
        };
    }

    public static IEnumerable<ApiGameRoomResponse> FromOldList(IEnumerable<GameRoom> oldList) => oldList.Select(FromOld)!;
}