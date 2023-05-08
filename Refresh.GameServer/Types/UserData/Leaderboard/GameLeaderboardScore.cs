using System.Xml.Serialization;

namespace Refresh.GameServer.Types.UserData.Leaderboard;

public class GameLeaderboardScore
{
    [XmlElement("mainPlayer")] public string Player { get; set; }
    [XmlElement("score")] public int Score { get; set; }
    [XmlElement("rank")] public int Rank { get; set; }
}