using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Reviews;

#nullable disable

[XmlRoot("slot")]
public class GameReviewSlot 
{
    [XmlAttribute("type")]
    public string SlotType { get; set; }

    [XmlText]
    public int SlotId { get; set; }
}