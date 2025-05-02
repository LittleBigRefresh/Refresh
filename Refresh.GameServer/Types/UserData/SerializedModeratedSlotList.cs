using System.Xml.Serialization;

namespace Refresh.Database.Models.Users;

#nullable disable

[XmlRoot("SlotIds")]
public class SerializedModeratedSlotList
{
    [XmlElement("Ids")]
    public List<int> Ids { get; set; }
}