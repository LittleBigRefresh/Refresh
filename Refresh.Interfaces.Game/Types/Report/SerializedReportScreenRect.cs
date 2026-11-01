using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Report;

#nullable disable

[XmlRoot("screenRect")]
public class SerializedReportScreenRect
{ 
    [XmlElement("rect")] 
    public SerializedReportRect Rect { get; set; } 
}