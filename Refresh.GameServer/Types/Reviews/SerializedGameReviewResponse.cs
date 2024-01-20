using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Reviews;

[XmlRoot("reviews")]
public class SerializedGameReviewResponse
{
    [XmlElement("review")]
    public List<SerializedGameReview> Reviews { get; set; }

    [XmlAttribute("hint")]
    public long Hint { get; set; }

    [XmlAttribute("hint_start")]
    public int HintStart { get; set; }
}