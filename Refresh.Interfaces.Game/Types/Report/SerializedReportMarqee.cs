using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Report;

#nullable disable

[XmlRoot("marqee")]
public class SerializedReportMarqee
{ 
    [XmlElement("rect")] 
    public SerializedReportRect Rect { get; set; } 
}