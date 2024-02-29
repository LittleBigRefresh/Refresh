using System.Xml.Serialization;
using Refresh.GameServer.Types.Matching;

namespace Refresh.GameServer.Types.Reviews;

[XmlRoot("slot")]
public class GameReviewSlot 
{
    [XmlAttribute("type")]
    public string SlotType { get; set; }

    [XmlText]
    public int SlotId { get; set; }
}