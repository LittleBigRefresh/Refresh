using System.Xml.Serialization;

namespace Refresh.GameServer.Types.UserData.Leaderboard;

#nullable disable

public class SerializedLeaderboardScore
{
    [XmlElement("mainPlayer")] public string Player { get; set; }
    [XmlElement("score")] public int Score { get; set; }
    [XmlElement("rank")] public int Rank { get; set; }
}