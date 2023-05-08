using System.Xml.Serialization;

namespace Refresh.GameServer.Types.UserData.Leaderboard;

[XmlRoot("playRecord")]
[XmlType("playRecord")]
public class GameScore
{
    [XmlElement("host")] public bool Host { get; set; }
    [XmlElement("type")] public byte ScoreType { get; set; } = 1; // Player count
    [XmlElement("score")] public int Score { get; set; }
}