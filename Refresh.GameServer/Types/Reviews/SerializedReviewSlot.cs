using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Reviews;

[XmlRoot("slot")]
public class SerializedReviewSlot 
{
    [XmlAttribute("type")]
    public string SlotType { get; set; } = "user";

    [XmlText]
    public int SlotId { get; set; }
}