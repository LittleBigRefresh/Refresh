using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.UserData;

#nullable disable

[XmlRoot("SlotIds")]
public class SerializedModeratedSlotList
{
    [XmlElement("Ids")]
    public List<int> Ids { get; set; }
}