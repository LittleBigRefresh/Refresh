using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot("marqee")]
public class Marqee : EmbeddedObject 
{ 
    [XmlElement("rect")] 
    public Rect Rect { get; set; } 
}