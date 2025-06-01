using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Report;

#nullable disable

[XmlRoot("infoBubble")]
public class InfoBubble
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