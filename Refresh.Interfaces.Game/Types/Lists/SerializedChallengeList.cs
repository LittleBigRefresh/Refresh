using System.Xml.Serialization;
using Refresh.Interfaces.Game.Types.Challenges.LbpHub;

namespace Refresh.Interfaces.Game.Types.Lists;

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