using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot(ElementName="slot")]
public class Slot : EmbeddedObject 
{ 
    [XmlElement(ElementName="id")] 
    public int Id { get; set; } 

    [XmlAttribute(AttributeName="type")] 
    public string Type { get; set; } 

    [XmlText] 
    public string Text { get; set; } 

    [XmlElement(ElementName="screenRect")] 
    public ScreenRect ScreenRect { get; set; } 

    [XmlElement(ElementName="slotId")] 
    public string SlotId { get; set; } 

    [XmlElement(ElementName="name")] 
    public string Name { get; set; } 

    [XmlElement(ElementName="icon")] 
    public string Icon { get; set; } 
}