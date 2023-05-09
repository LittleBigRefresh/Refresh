using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

#nullable disable

[XmlRoot("screenRect")]
public partial class ScreenRect : IEmbeddedObject 
{ 
    [XmlElement("rect")] 
    public Rect Rect { get; set; } 
}