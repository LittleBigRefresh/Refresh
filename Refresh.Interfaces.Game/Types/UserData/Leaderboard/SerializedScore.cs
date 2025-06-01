using System.Xml.Serialization;
using Refresh.Database.Query;

namespace Refresh.Interfaces.Game.Types.UserData.Leaderboard;

[XmlRoot("playRecord")]
[XmlType("playRecord")]
public class SerializedScore : ISerializedScore
{
    [XmlElement("host")] public bool Host { get; set; }
    [XmlElement("type")] public byte ScoreType { get; set; } = 1; // Player count
    [XmlElement("score")] public int Score { get; set; }
}