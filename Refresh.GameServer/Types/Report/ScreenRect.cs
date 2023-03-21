using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot(ElementName="screenRect")]
public class ScreenRect : EmbeddedObject 
{ 
    [XmlElement(ElementName="rect")] 
    public Rect Rect { get; set; } 
}