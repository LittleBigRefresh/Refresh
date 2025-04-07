using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Challenges.LbpHub;

[XmlRoot("criterion")]
[XmlType("criterion")]
public class SerializedChallengeCriterion
{
    /// <summary>
    /// The challenge's criteria type (time/score/lives etc).
    /// </summary>
    /// <seealso cref="GameChallengeCriteriaType"/>
    [XmlAttribute("name")] public byte Type { get; set; } = 0;
    /// <summary>
    /// Appears to always be 0 when sent by the game, does not affect anything either.
    /// </summary>
    [XmlText] public long Value { get; set; } = 0;
}