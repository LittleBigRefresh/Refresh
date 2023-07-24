using AttribDoc.Attributes;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Services;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class StatisticsApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("statistics"), Authentication(false)]
    [DocSummary("Retrieves various information about the Refresh instance.")]
    public ApiResponse<ApiStatisticsResponse> GetStatistics(RequestContext context, GameDatabaseContext database, MatchService match) 
        =>new ApiStatisticsResponse
        {
            TotalLevels = database.GetTotalLevelCount(),
            TotalUsers = database.GetTotalUserCount(),
            CurrentRoomCount = match.Rooms.Count(),
            CurrentIngamePlayersCount = match.TotalPlayers,
        };
}