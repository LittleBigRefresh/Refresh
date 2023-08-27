using System.Xml.Serialization;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Roles;

namespace Refresh.GameServer.Endpoints.Game;

public class PresenceEndpoints : EndpointGroup
{
    [GameEndpoint("playersInPodCount")]
    [MinimumRole(GameUserRole.Restricted)]
    public int TotalPlayersInPod(RequestContext context, MatchService match) => match.TotalPlayersInPod;

    [GameEndpoint("totalPlayerCount")]
    [MinimumRole(GameUserRole.Restricted)]
    public int TotalPlayers(RequestContext context, MatchService match) => match.TotalPlayers;

    [GameEndpoint("planetStats", Method.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLevelStatisticsResponse GetLevelStatistics(RequestContext context, GameDatabaseContext database) => new()
    {
        TotalLevels = database.GetTotalLevelCount(),
        TotalTeamPicks = database.GetTotalTeamPickCount(),
    };

    [XmlRoot("planetStats")]
    public class SerializedLevelStatisticsResponse
    {
        [XmlElement("totalSlotCount")]
        public int TotalLevels { get; set; }
        [XmlElement("mmPicksCount")]
        public int TotalTeamPicks { get; set; }
    }
}