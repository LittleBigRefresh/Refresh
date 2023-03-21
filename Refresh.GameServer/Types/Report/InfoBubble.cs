using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot(ElementName="infoBubble")]
public class InfoBubble : EmbeddedObject 
{ 
    [XmlElement(ElementName="slot")] 
    public Slot Slot { get; set; } 

    [XmlElement(ElementName="name")] 
    public string Name { get; set; } 

    [XmlElement(ElementName="description")] 
    public string Description { get; set; } 

    [XmlAttribute(AttributeName="type")] 
    public string Type { get; set; } 

    [XmlText] 
    public string Text { get; set; } 
}