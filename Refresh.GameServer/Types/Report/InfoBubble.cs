using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot("infoBubble")]
public class InfoBubble : EmbeddedObject 
{ 
    [XmlElement("slot")] 
    public Slot Slot { get; set; } 

    [XmlElement("name")] 
    public string Name { get; set; } 

    [XmlElement("description")] 
    public string Description { get; set; } 

    [XmlAttribute(AttributeName="type")] 
    public string Type { get; set; } 

    [XmlText] 
    public string Text { get; set; } 
}