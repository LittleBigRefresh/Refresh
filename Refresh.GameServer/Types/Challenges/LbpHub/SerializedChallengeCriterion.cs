using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Challenges.LbpHub;

[XmlRoot("criterion")]
[XmlType("criterion")]
public class SerializedChallengeCriterion
{
    /// <summary>
    /// The challenge's criterion type.
    /// </summary>
    /// <seealso cref="GameChallengeType"/>
    [XmlAttribute("name")] public byte Type { get; set; } = 0;
    /// <summary>
    /// Appears to always be 0 when sent by the game, therefore we don't need to save it.
    /// </summary>
    [XmlText] public long Value { get; set; }
}