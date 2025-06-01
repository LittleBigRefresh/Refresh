using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Report;

#nullable disable

[XmlRoot("marqee")]
public class Marqee
{ 
    [XmlElement("rect")] 
    public Rect Rect { get; set; } 
}