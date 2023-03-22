using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot("screenRect")]
public class ScreenRect : EmbeddedObject 
{ 
    [XmlElement("rect")] 
    public Rect Rect { get; set; } 
}