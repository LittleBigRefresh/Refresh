using System.Xml.Serialization;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class PresenceEndpoints : EndpointGroup
{
    [GameEndpoint("playersInPodCount")]
    [RequiresAuthentication]
    public int PlayersInPod(RequestContext context) => 1;

    [GameEndpoint("totalPlayerCount")]
    [RequiresAuthentication]
    public int TotalPlayers(RequestContext context) => 1;

    [GameEndpoint("planetStats", Method.Get, ContentType.Xml)]
    [RequiresAuthentication]
    public LevelStatistics GetLevelStatistics(RequestContext context) => new()
    {
        TotalTeamPicks = 0,
        TotalLevels = 0,
    };

    [XmlRoot("planetStats")]
    public class LevelStatistics
    {
        [XmlElement("totalSlotCount")]
        public int TotalLevels { get; set; }
        [XmlElement("mmPicksCount")]
        public int TotalTeamPicks { get; set; }
    }
}