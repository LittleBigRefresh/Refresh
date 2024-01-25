using System.Xml.Serialization;
using Refresh.GameServer.Types.Lists;

namespace Refresh.GameServer.Types.Reviews;

#nullable disable

[XmlRoot("reviews")]
public class SerializedGameReviewResponse(List<SerializedGameReview> items) : SerializedList<SerializedGameReview>
{
    [XmlElement("review")]
    public sealed override List<SerializedGameReview> Items { get; set; } = items;

    [XmlAttribute("hint")]
    public string Hint { get; set; } = string.Empty;
}