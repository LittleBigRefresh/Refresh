using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot(ElementName="marqee")]
public class Marqee : EmbeddedObject 
{ 
    [XmlElement(ElementName="rect")] 
    public Rect Rect { get; set; } 
}