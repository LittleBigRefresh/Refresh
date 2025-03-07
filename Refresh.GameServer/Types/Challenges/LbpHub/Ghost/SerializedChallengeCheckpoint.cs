using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Challenges.LbpHub.Ghost;

[XmlRoot("checkpoint")]
[XmlType("checkpoint")]
public class SerializedChallengeCheckpoint
{
    [XmlAttribute("uid")] public int Uid { get; set; }
    /// <summary>
    /// When this checkpoint was activated.
    /// </summary>
    [XmlAttribute("time")] public long Time { get; set; }

    // currently not needed
    //[XmlElement("metric")] public List<SerializedChallengeCheckpointMetric> Metrics { get; set; } = [];
}