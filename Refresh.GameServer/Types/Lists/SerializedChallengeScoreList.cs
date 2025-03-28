using System.Xml.Serialization;
using Refresh.GameServer.Types.Challenges.LbpHub;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("challenge-scores")]
[XmlType("challenge-scores")]
public class SerializedChallengeScoreList
{
    public SerializedChallengeScoreList() {}

    public SerializedChallengeScoreList(IEnumerable<SerializedChallengeScore> items)
    {
        this.Items = items.ToList();
    }
    
    [XmlElement("challenge-score")]
    public List<SerializedChallengeScore> Items { get; set; } = [];
}