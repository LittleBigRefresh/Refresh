using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Reviews;

#nullable disable

[XmlRoot("slot")]
public class GameReviewSlot 
{
    [XmlAttribute("type")]
    public string SlotType { get; set; }

    [XmlText]
    public int SlotId { get; set; }
}