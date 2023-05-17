using System.Xml.Serialization;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;

namespace Refresh.GameServer.Endpoints.Game;

public class PresenceEndpoints : EndpointGroup
{
    [GameEndpoint("playersInPodCount")]
    public int TotalPlayersInPod(RequestContext context, MatchService match) => match.TotalPlayersInPod;

    [GameEndpoint("totalPlayerCount")]
    public int TotalPlayers(RequestContext context, MatchService match) => match.TotalPlayers;

    [GameEndpoint("planetStats", Method.Get, ContentType.Xml)]
    public SerializedLevelStatisticsResponse GetLevelStatistics(RequestContext context, GameDatabaseContext database) => new()
    {
        TotalLevels = database.GetTotalLevelCount(),
        TotalTeamPicks = 0,
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