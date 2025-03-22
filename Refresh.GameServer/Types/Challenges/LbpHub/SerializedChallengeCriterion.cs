using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Challenges.LbpHub;

[XmlRoot("criterion")]
[XmlType("criterion")]
public class SerializedChallengeCriterion
{
    /// <summary>
    /// Whether this is a score/time/lives etc challenge.
    /// </summary>
    /// <seealso cref="GameChallengeType"/>
    [XmlAttribute("name")] public byte Type { get; set; } = 0;
    /// <summary>
    /// Appears to always be 0 when sent by the game, does not affect anything either.
    /// </summary>
    [XmlText] public long Value { get; set; } = 0;
}