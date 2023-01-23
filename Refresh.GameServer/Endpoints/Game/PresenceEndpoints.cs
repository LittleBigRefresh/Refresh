using System.Xml.Serialization;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;

namespace Refresh.GameServer.Endpoints.Game;

public class PresenceEndpoints : EndpointGroup
{
    [GameEndpoint("playersInPodCount")]
    public int PlayersInPod(RequestContext context) => 1;

    [GameEndpoint("totalPlayerCount")]
    public int TotalPlayers(RequestContext context) => 1;

    [GameEndpoint("planetStats", Method.Get, ContentType.Xml)]
    public LevelStatistics GetLevelStatistics(RequestContext context, RealmDatabaseContext database) => new()
    {
        TotalLevels = database.GetTotalLevelCount(),
        TotalTeamPicks = 0,
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