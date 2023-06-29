using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Newtonsoft.Json;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;

namespace Refresh.GameServer.Endpoints.ApiV2;

public class StatisticsApiEndpoints : EndpointGroup
{
    [ApiV2Endpoint("statistics")]
    [Authentication(false)]
    public StatisticsResponse GetStatistics(RequestContext context, MatchService match, GameDatabaseContext database) =>
        new()
        {
            TotalLevels = database.GetTotalLevelCount(),
            TotalUsers = database.GetTotalUserCount(),
            CurrentRoomCount = match.Rooms.Count(),
            CurrentIngamePlayersCount = match.TotalPlayers,
        };

    [JsonObject]
    public class StatisticsResponse
    {
        public int TotalLevels { get; set; }
        public int TotalUsers { get; set; }
        public int CurrentRoomCount { get; set; }
        public int CurrentIngamePlayersCount { get; set; }
    }
}