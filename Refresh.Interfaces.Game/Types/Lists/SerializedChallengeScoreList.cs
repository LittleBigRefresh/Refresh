using System.Xml.Serialization;
using Refresh.Interfaces.Game.Types.Challenges.LbpHub;

namespace Refresh.Interfaces.Game.Types.Lists;

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