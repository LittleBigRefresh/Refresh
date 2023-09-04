using System.Xml.Serialization;

namespace Refresh.GameServer.Types.UserData;

#nullable disable

[XmlRoot("SlotIds")]
public class SerializedModeratedSlotList
{
    [XmlElement("Ids")]
    public List<int> Ids { get; set; }
}