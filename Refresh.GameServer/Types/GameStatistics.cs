namespace Refresh.GameServer.Types;

[JsonObject(MemberSerialization.OptOut)]
public class GameStatistics
{
    public int TotalLevels { get; set; }
    public int TotalUsers { get; set; }
    public int CurrentRoomCount { get; set; }
    public int CurrentIngamePlayersCount { get; set; }
}