using Refresh.GameServer.Types.Matching;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameRoomPlayerResponse : IApiResponse, IDataConvertableFrom<ApiGameRoomPlayerResponse, GameRoomPlayer>
{
    public required string Username { get; set; }
    public required string? UserId { get; set; }
    
    public static ApiGameRoomPlayerResponse? FromOld(GameRoomPlayer? old)
    {
        if (old is null) return null;

        return new ApiGameRoomPlayerResponse
        {
            Username = old.Username,
            UserId = old.Id?.ToString(),
        };
    }

    public static IEnumerable<ApiGameRoomPlayerResponse> FromOldList(IEnumerable<GameRoomPlayer> oldList) => oldList.Select(FromOld).ToList()!;
}