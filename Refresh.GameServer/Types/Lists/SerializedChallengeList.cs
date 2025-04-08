using System.Xml.Serialization;
using Refresh.GameServer.Types.Challenges.LbpHub;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("challenge")]
[XmlType("challenges")]
public class SerializedChallengeList
{
    public SerializedChallengeList() {}

    public SerializedChallengeList(IEnumerable<SerializedChallenge> challenges)
    {
        this.Items = challenges.ToList();
    }
    
    [XmlElement("challenge")]
    public List<SerializedChallenge> Items { get; set; } = [];
}