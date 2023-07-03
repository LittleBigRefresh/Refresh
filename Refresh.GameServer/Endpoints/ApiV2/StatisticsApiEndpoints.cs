using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Newtonsoft.Json;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types;

namespace Refresh.GameServer.Endpoints.ApiV2;

public class StatisticsApiEndpoints : EndpointGroup
{
    [ApiV2Endpoint("statistics")]
    [Authentication(false)]
    public GameStatistics GetStatistics(RequestContext context, MatchService match, GameDatabaseContext database) =>
        new()
        {
            TotalLevels = database.GetTotalLevelCount(),
            TotalUsers = database.GetTotalUserCount(),
            CurrentRoomCount = match.Rooms.Count(),
            CurrentIngamePlayersCount = match.TotalPlayers,
        };
}