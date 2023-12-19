using Refresh.GameServer.Authentication;

namespace Refresh.GameServer.Types.Matching;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class RoomStatistics
{
    public ushort PlayerCount { get; set; }
    public ushort PlayersInPodCount { get; set; }
    public ushort RoomCount { get; set; }
    public Dictionary<TokenGame, ushort> PerGame { get; set; }
    public Dictionary<TokenPlatform, ushort> PerPlatform { get; set; }
}