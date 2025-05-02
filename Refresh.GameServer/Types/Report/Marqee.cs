using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Report;

#nullable disable

[XmlRoot("marqee")]
public class Marqee
{ 
    [XmlElement("rect")] 
    public Rect Rect { get; set; } 
}