using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Challenges.LbpHub.Ghost;

[XmlRoot("checkpoint")]
[XmlType("checkpoint")]
public class SerializedChallengeCheckpoint
{
    /// <summary>
    /// The UID of this particular checkpoint. If multiple checkpoints with the same UID appear in the SerializedChallengeGhost,
    /// it means the same checkpoint has been activated several times.
    /// </summary>
    [XmlAttribute("uid")] public int Uid { get; set; }
    /// <summary>
    /// When this checkpoint was activated, in unix seconds.
    /// </summary>
    [XmlAttribute("time")] public long Time { get; set; }

    // currently not needed
    //[XmlElement("metric")] public List<SerializedChallengeCheckpointMetric> Metrics { get; set; } = [];
}