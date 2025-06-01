using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Matching;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users.Rooms;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameRoomPlayerResponse : IApiResponse, IDataConvertableFrom<ApiGameRoomPlayerResponse, GameRoomPlayer>
{
    public required string Username { get; set; }
    public required string? UserId { get; set; }
    
    public static ApiGameRoomPlayerResponse? FromOld(GameRoomPlayer? old, DataContext dataContext)
    {
        if (old is null) return null;

        return new ApiGameRoomPlayerResponse
        {
            Username = old.Username,
            UserId = old.Id?.ToString(),
        };
    }

    public static IEnumerable<ApiGameRoomPlayerResponse> FromOldList(IEnumerable<GameRoomPlayer> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}