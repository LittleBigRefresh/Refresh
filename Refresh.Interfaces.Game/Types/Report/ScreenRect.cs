using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Report;

#nullable disable

[XmlRoot("screenRect")]
public class ScreenRect
{ 
    [XmlElement("rect")] 
    public Rect Rect { get; set; } 
}