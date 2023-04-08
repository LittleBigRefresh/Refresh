using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

#nullable disable

[XmlRoot("screenRect")]
public class ScreenRect : EmbeddedObject 
{ 
    [XmlElement("rect")] 
    public Rect Rect { get; set; } 
}