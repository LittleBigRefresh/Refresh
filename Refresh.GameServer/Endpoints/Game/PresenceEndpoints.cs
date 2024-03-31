using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Roles;

namespace Refresh.GameServer.Endpoints.Game;

public class PresenceEndpoints : EndpointGroup
{
    [GameEndpoint("playersInPodCount")]
    [MinimumRole(GameUserRole.Restricted)]
    public int TotalPlayersInPod(RequestContext context, MatchService match) => match.RoomAccessor.GetStatistics().PlayersInPodCount;

    [GameEndpoint("totalPlayerCount")]
    [MinimumRole(GameUserRole.Restricted)]
    public int TotalPlayers(RequestContext context, MatchService match) => match.RoomAccessor.GetStatistics().PlayerCount;

    [GameEndpoint("planetStats/highestSlotId")]
    [MinimumRole(GameUserRole.Restricted)]
    public int GetTotalLevelCount(RequestContext context, GameDatabaseContext database, Token token) => database.GetTotalLevelCount(token.TokenGame);
    
    [GameEndpoint("planetStats", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLevelStatisticsResponse GetLevelStatistics(RequestContext context, GameDatabaseContext database, Token token) => new()
    {
        TotalLevels = database.GetTotalLevelCount(token.TokenGame),
        TotalTeamPicks = database.GetTotalTeamPickCount(token.TokenGame),
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