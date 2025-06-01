using System.Xml.Serialization;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Types.Reviews;

#nullable disable

[XmlRoot("reviews")]
public class SerializedGameReviewResponse : SerializedList<SerializedGameReview>
{
    public SerializedGameReviewResponse() {}
    
    public SerializedGameReviewResponse(List<SerializedGameReview> items)
    {
        this.Items = items;
    }
    [XmlElement("review")]
    public sealed override List<SerializedGameReview> Items { get; set; } = null!;

    [XmlAttribute("hint")]
    public string Hint { get; set; } = string.Empty;
}